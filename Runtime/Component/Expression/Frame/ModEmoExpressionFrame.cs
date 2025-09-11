namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Frame")]
    internal sealed class ModEmoExpressionFrame : ModEmoTagComponent, IModEmoExpressionFrameProvider
    {
        public float Time;

        public IEnumerable<ExpressionFrame> GetFrames()
            => gameObject.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>().SelectMany(x => x.GetFrames()).Select(x => ExpressionFrame.Create(this, Time, x.GetBlendShapes()));

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(Time);
            foreach (var x in GetFrames())
            {
                hashCode.Add(x);
            }
        }
    }
}