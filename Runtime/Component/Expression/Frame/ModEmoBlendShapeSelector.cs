
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape")]
    [CanEditMultipleObjects]
    internal sealed class ModEmoBlendShapeSelector : ModEmoTagComponent, IModEmoExpressionFrameProvider
    {
        public List<BlendShape> BlendShapes = new();

        public IEnumerable<ExpressionFrame> GetFrames()
        {
            yield return ExpressionFrame.Create(this, 0, BlendShapes);
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(isActiveAndEnabled);
            foreach (var x in BlendShapes.AsSpan())
            {
                hashCode.Add(x);
            }
        }
    }
}