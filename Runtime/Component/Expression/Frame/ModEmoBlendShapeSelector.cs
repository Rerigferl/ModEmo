

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape")]
    internal sealed class ModEmoBlendShapeSelector : ModEmoTagComponent, IModEmoBlendShapeProvider
    {
        public float Keyframe = 0;
        public List<BlendShape> BlendShapes = new();

        public void CollectBlendShapes(in BlendShapeCurveWriter writer)
        {
            foreach (var blendShape in BlendShapes)
            {
                if (Keyframe != 0)
                    writer.Write(0, blendShape with { Value = 0 });
                writer.Write(Keyframe, blendShape);
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