using System.Collections.Immutable;
using System.Text.RegularExpressions;
using nadena.dev.ndmf.util;
using Numeira.Animation;
using static Numeira.Curve;
namespace Numeira;

internal sealed class ModEmoData
{
    private const string UncategorizedGroupName = "Uncategorized";

    public SkinnedMeshRenderer Face { get; }

    public List<ExpressionData>? Expressions { get; set; }

    public FaceInfo FaceInfo { get; }

    public HashSet<AvatarParameter> Parameters { get; } = new(AvatarParameter.ParameterNameEqualityComparer.Instance);

    public MotionBuilder BlankClip { get; } = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath("3107326e8ebb7da42981f107a7207199"));

    private readonly Dictionary<int, int> blendshapeIndexForSynchronize = new();

    internal static ModEmoData Init(BuildContext context) => new(context);

    private ModEmoData(BuildContext context)
    {
        var component = context.GetModEmoContext().Root;
        Face = component.GetFaceRenderer() ?? throw new MissingReferenceException("Face object is missing");

        var mesh = Face.sharedMesh;
        FaceInfo = new(component);

        List<IModEmoExpression> expressions = new();
        foreach (var x in component.ExportExpressions())
        {
            expressions.Add(x.Key);
            foreach (var y in x)
            {
                expressions.Add(y);
            }
        }

        if (component.GetBlinkExpression() is { } blink)
            expressions.Add(blink);

        var collector = new BlendshapeCollectorRegistry(OnRegisterBlendshape);
        void OnRegisterBlendshape(BlendshapeControlType type, Transform target, string name, float value)
        {
            if (target != Face.transform)
                return;

            if (!FaceInfo.BlendshapeMap.TryGetValue(name, out var info))
                return;

            if (type is BlendshapeControlType.Normal)
            {
                if (Mathf.Approximately(info.Value, value))
                    return;
                info.UsageInfo.ControlGateLayers[0] = true;
            }
            else
            {
                info.UsageInfo.CancelGateLayers[0] = true;
            }

            info.UsageInfo.UseOverrideGate = true;
        }

        foreach (var x in expressions)
            x.RegisterAnimations(collector, new() { AnimationName = "", AvatarRootTransform = context.AvatarRootTransform, FaceObject = Face.transform });

        foreach (var x in FaceInfo.BlendShapes)
        {
            if (x.UsageInfo.UseCancelGate && Mathf.Approximately(x.Value, 0) && !x.UsageInfo.UseControlGate)
            {
                x.UsageInfo.CancelGateLayers.Clear();
            }
        }

        if (component.MouthMorphCanceller is { } mmc)
        {
            // foreach(var x in mmc.GetUsedBlendshapes())
            // {
            //     if (FaceInfo.BlendshapeMap.TryGetValue(x.Name, out var info))
            //         info.UsageInfo.UseEnableGate = true;
            // }
        }

        if (component.GetComponentInDirectChildren<IModEmoRuntimeBlendshapeController>(includeSelf: true) is { } rbc)
        {
            foreach(var pattern in rbc.Blacklist)
            {
                if (string.IsNullOrEmpty(pattern)) continue;
                var regex = new Regex(pattern, RegexOptions.CultureInvariant);

                foreach(var x in FaceInfo.BlendShapes)
                {
                    if (regex.IsMatch(x.Name))
                        x.UsageInfo.UseOverrideGate = false;
                }
            }

            foreach (var x in rbc.Component.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(includeSelf: true))
            {
                foreach (var blendShape in x.GetUsedBlendshapes())
                {
                    if (FaceInfo.BlendshapeMap.TryGetValue(blendShape.Name, out var info))
                        info.UsageInfo.UseOverrideGate = true;
                }
            }
        }
    }

    public int GetBlendshapeIndexForSync(int blendshapeIndex) => blendshapeIndexForSynchronize.GetOrAdd(blendshapeIndex, _ => blendshapeIndexForSynchronize.Count + 1);

    [Obsolete]
    public static ImmutableDictionary<string, BlendShapeInfo> GetBlendShapeInfos(SkinnedMeshRenderer? renderer)
    {
        if (renderer is null)
            return ImmutableDictionary<string, BlendShapeInfo>.Empty;

        var mesh = renderer.sharedMesh;
        int count = mesh.blendShapeCount;

        Dictionary<string, BlendShapeInfo> info = new();
        for (int i = 0; i < count; i++)
        {
            var name = mesh.GetBlendShapeName(i);
            info.TryAdd(name, new(renderer, i));
        }
        return info.ToImmutableDictionary();
    }

    [Obsolete]
    public static (List<KeyValuePair<string, List<string>>> CategorizedBlendShapeNames, ImmutableDictionary<string, BlendShapeInfo> BlendShapeInfos)? GetCategorizedBlendShapes(ModEmo component)
    {
        var face = component.GetFaceRenderer();
        var mesh = face?.sharedMesh;
        if (face == null || mesh == null)
            return default;

        List<KeyValuePair<string, List<string>>> groups = new();
        string currentGroup = $"{UncategorizedGroupName}";
        List<string> currentGroupList = new();

        groups.Add(KeyValuePair.Create(currentGroup, currentGroupList));

        Dictionary<string, BlendShapeInfo> blendShapes = new();

        var regex = component.Settings.SeparatorStringRegEx;
        int count = mesh.blendShapeCount;
        for (int i = 0; i < count; i++)
        {
            var name = mesh.GetBlendShapeName(i);
            if (regex.IsMatch(name))
            {
                currentGroup = $"{regex.Replace(name, "")}";
                currentGroupList = new();
                groups.Add(KeyValuePair.Create(currentGroup, currentGroupList));
                continue;
            }

            blendShapes.TryAdd(name, new(face, i));
            currentGroupList.Add(name);
        }

        return (groups, blendShapes.ToImmutableDictionary());

    }
}

internal sealed class BlendshapeCollectorRegistry : IAnimationRegistry
{
    public delegate void CallbackDelegate(BlendshapeControlType type, Transform target,string name, float value);

    private readonly Context context;

    public BlendshapeCollectorRegistry(CallbackDelegate callback)
    {
        this.context = new Context(callback);
    }

    public IKeyframeWriterContext RegisterAnimation(in AnimationGeneratorOptions options, string? blendParameter = null)
        => context;

    public (IKeyframeWriterContext X, IKeyframeWriterContext Y) RegisterTwoAxisAnimation(in AnimationGeneratorOptions options, string blendParameterX, string blendParameterY) 
        => (context, context);

    public IKeyframeWriterContext RegisterMultipleConditionAnimation(in AnimationGeneratorOptions options, params string[] blendParameters)
        => context;

    private sealed class Context : IKeyframeWriterContext
    {
        private readonly CallbackDelegate callback;

        public Context(CallbackDelegate callback)
        {
            this.callback = callback;
        }

        public void AddBlendshape(Transform target, string name, float time, float value)
        {
            callback(BlendshapeControlType.Normal, target, name, value);
        }

        public void AddCancelBlendshape(Transform target, string name, float time, float value)
        {
            callback(BlendshapeControlType.Cancel, target, name, value);
        }

        public void AddRotation(Transform target, float time, Vector3 eularAngle, bool relative = true)
        {

        }

        public void SetAnimatorParameter<T>(string name, float time, T value)
        {

        }

        public void SetAvatarParameter<T>(string name, T value)
        {

        }
    }
}
