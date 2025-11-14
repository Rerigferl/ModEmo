

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape")]
    internal sealed class ModEmoBlendShapeSelector : ModEmoTagComponent, IModEmoBlendShapeProvider, IModEmoAnimationProvider
    {
        public float Keyframe = 0;
        public List<BlendShape> BlendShapes = new();

        public IEnumerable<BlendShape> GetBlendShapes() => BlendShapes;

        public void WriteAnimation(IAnimationWriter writer, in AnimationWriterContext context)
        {
            foreach (var blendShape in BlendShapes.AsSpan())
            {
                var binding = new AnimationBinding(typeof(SkinnedMeshRenderer), context.FaceObjectPath ?? "", $"{(blendShape.Cancel ? "cancel." : "")}blendShape.{blendShape.Name}");
               
                writer.WriteDefaultValue(binding, 0);
                writer.Write(binding, Keyframe, blendShape.Value);
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