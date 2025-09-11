namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Frame Folder")]
    internal sealed class ModEmoExpressionFrameFolder : ModEmoTagComponent, IModEmoExpressionFrameProvider
    {
        public IEnumerable<ExpressionFrame> GetFrames() => gameObject.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>().SelectMany(x => x.GetFrames());

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in GetFrames())
            {
                hashCode.Add(x);
            }
        }
    }
}