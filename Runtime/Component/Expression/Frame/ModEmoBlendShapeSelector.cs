

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
                if (Keyframe != 0)
                {
                    writer.Write(binding, 0, 0);
                }
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
}