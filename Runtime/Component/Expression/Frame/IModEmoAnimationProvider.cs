namespace Numeira;

internal interface IModEmoAnimationProvider : IModEmoComponent
{
    void WriteAnimation(IAnimationWriter writer, in AnimationWriterContext context);
}

internal interface IModEmoAnimationCollector : IModEmoComponent
{
    public IEnumerable<IModEmoAnimationProvider> GetAnimationProviders()
    {
        foreach(var component in Component.GetComponentsInDirectChildren<IModEmoAnimationProvider>(includeSelf: true))
        {
            var collectors = component.GameObject.GetComponents<IModEmoAnimationCollector>();
            foreach (var collector in collectors)
            {
                if (collector != this)
                    goto Continue;
            }

            yield return component;

        Continue:
            { }
        }
    }

    public void CollectAnimation(IAnimationWriterSource source, in AnimationWriterContext context)
    {
        foreach(var child in GetAnimationProviders())
        {
            child.WriteAnimation(source, context);
        }
    }
}

internal interface IAnimationWriter
{
    public delegate void PreWriteKeyframeDelegate(ref AnimationBinding binding, ref Curve.Keyframe keyframe);

    public event PreWriteKeyframeDelegate? PreWriteKeyframe;

    public bool UpdateKeyframe { get; set; }

    public void Write(AnimationBinding binding, float keyframe, float value) => Write(binding, new Curve.Keyframe(keyframe, value));

    public void Write(AnimationBinding binding, Curve.Keyframe keyframe);
}

internal interface IAnimationWriterSource : IAnimationWriter
{

}

internal abstract class AnimationWriter : IAnimationWriterSource, IAnimationWriter
{
    public static DefaultAnimationWriter Shared { get; } = new();
    public bool UpdateKeyframe { get; set; } = false;

    public event IAnimationWriter.PreWriteKeyframeDelegate? PreWriteKeyframe;

    protected abstract void Write(AnimationBinding binding, Curve.Keyframe keyframe);

    void IAnimationWriter.Write(AnimationBinding binding, Curve.Keyframe keyframe)
    {
        PreWriteKeyframe?.Invoke(ref binding, ref keyframe);

        Write(binding, keyframe);
    }

    public sealed class DefaultAnimationWriter : AnimationWriter
    {
        public Dictionary<AnimationBinding, Curve> Curves { get; } = new();

        protected override void Write(AnimationBinding binding, Curve.Keyframe keyframe)
        {
            Curves.GetOrAdd(binding, _ => new()).AddKey(keyframe, UpdateKeyframe);
        }
    }
}

internal abstract class BlendshapeCollector : AnimationWriter
{
    protected override void Write(AnimationBinding binding, Curve.Keyframe keyframe)
    {
        if (binding.Type != typeof(SkinnedMeshRenderer))
            return;

        const string cancelShapeNamePrefix = "cancel.";
        const string blendShapeNamePrefix = "blendShape.";
        var name = binding.PropertyName.AsSpan();
        bool isCancel = false;
        if (name.StartsWith(cancelShapeNamePrefix))
        {
            isCancel = true;
            name = name[cancelShapeNamePrefix.Length..];
        }

        if (!name.StartsWith(blendShapeNamePrefix))
            return;

        name = name[blendShapeNamePrefix.Length..];

        WriteWithBlendshape(binding, keyframe, name, isCancel);
    }

    protected abstract void WriteWithBlendshape(AnimationBinding binding, Curve.Keyframe keyframe, ReadOnlySpan<char> blendShapeName, bool isCancel);
}

internal struct AnimationBinding
{
    public AnimationBinding(Type type, string path, string propertyName)
    {
        Type = type;
        Path = path;
        PropertyName = propertyName;
    }

    public Type Type { get; set; }
    public string Path {  get; set; }
    public string PropertyName { get; set; }

    internal static AnimationBinding Parameter(string parameterName)
    {
        return new AnimationBinding(typeof(Animator), "", parameterName);
    }

#if UNITY_EDITOR

    public static implicit operator EditorCurveBinding(in AnimationBinding binding)
    {
        return new EditorCurveBinding()
        {
            type = binding.Type,
            path = binding.Path,
            propertyName = binding.PropertyName,
        };
    }

    public static implicit operator AnimationBinding(in EditorCurveBinding binding)
    {
        return new AnimationBinding()
        {
            Type = binding.type,
            Path = binding.path,
            PropertyName = binding.propertyName,
        };
    }

#endif
}

internal readonly struct AnimationWriterContext
{
    public AnimationWriterContext(Transform avatarRootTransform, Transform faceObjectTransform, string faceObjectPath)
    {
        AvatarRootTransform = avatarRootTransform;
        FaceObjectTransform = faceObjectTransform;
        FaceObjectPath = faceObjectPath;
    }

    public Transform? AvatarRootTransform { get; init; }
    public Transform? FaceObjectTransform { get; init; }
    public string? FaceObjectPath { get; init; }
}

internal static class AnimationWriterExt
{
    public static EventUnsubscriber RegisterPreWriteKeyframe(this IAnimationWriter writer, IAnimationWriter.PreWriteKeyframeDelegate @delegate)
    {
        writer.PreWriteKeyframe += @delegate;
        return new EventUnsubscriber(() => writer.PreWriteKeyframe -= @delegate);
    }
}

internal readonly ref struct EventUnsubscriber
{
    private readonly Action? action;

    public static EventUnsubscriber Empty => default;

    public EventUnsubscriber(Action action) => this.action = action;

    public void Dispose()
    {
        action?.Invoke();
    }
}