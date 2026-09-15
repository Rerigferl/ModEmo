
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape")]
    internal sealed class ModEmoBlendShapeSelector : ModEmoTagComponent, IBlendshapeWriterComponent
    {
        public float Keyframe = 0;
        public List<BlendShape> BlendShapes = new();

        public void OnEnable() { }

        public void WriteKeyframes(IKeyframeWriterContext context, in AnimationGeneratorOptions options)
        {
            if (!enabled)
                return;

            foreach (var blendShape in BlendShapes.AsSpan())
            {
                if (!blendShape.Cancel)
                {
                    context.AddBlendshapeDefault(options.FaceObject, blendShape.Name);
                    context.AddBlendshape(options.FaceObject, blendShape.Name, Keyframe, blendShape.Value);
                }
                else
                {
                    context.AddCancelBlendshapeDefault(options.FaceObject, blendShape.Name);
                    context.AddCancelBlendshape(options.FaceObject, blendShape.Name, Keyframe, blendShape.Value);
                }
            }
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(isActiveAndEnabled);
            hashCode.Add(Keyframe);
            foreach (var x in BlendShapes.AsSpan())
            {
                hashCode.Add(x);
            }
        }
    }

#if UNITY_EDITOR
    internal static class ModEmoBlendShapeSelectorExt
    {
        public static void ImportFromAnimationClip(this ModEmoBlendShapeSelector selector, AnimationClip? clip)
        {
            if (clip == null)
                return;

            var bindings = AnimationUtility.GetCurveBindings(clip);
            foreach(var binding in bindings)
            {
                if (binding.type != typeof(SkinnedMeshRenderer))
                    continue;
                var name = binding.propertyName;
                if (!name.StartsWith("blendShape."))
                    continue;
                name = name["blendShape.".Length..];

                var curve = AnimationUtility.GetEditorCurve(clip, binding);
                selector.BlendShapes.Add(new() { Name = name, Value = curve.Evaluate(0), Cancel = false });
            }
        }
    }
#endif
}