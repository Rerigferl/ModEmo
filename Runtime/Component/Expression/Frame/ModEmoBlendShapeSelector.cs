

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape")]
    internal sealed class ModEmoBlendShapeSelector : ModEmoTagComponent, IModEmoExpressionFrameProvider
    {
        public float Keyframe = 0;
        public List<BlendShape> BlendShapes = new();

        public IEnumerable<ExpressionFrame> GetFrames()
        {
            yield return ExpressionFrame.Create(this, Keyframe, BlendShapes);
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