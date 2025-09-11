namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape Selector")]
    internal sealed class ModEmoBlendShapeSelector : ModEmoTagComponent, IModEmoExpressionFrameProvider
    {
        public BlendShape[] BlendShapes = Array.Empty<BlendShape>();

        public IEnumerable<ExpressionFrame> GetFrames()
        {
            yield return ExpressionFrame.Create(this, 0, BlendShapes);
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in BlendShapes)
            {
                hashCode.Add(x);
            }
        }
    }
}