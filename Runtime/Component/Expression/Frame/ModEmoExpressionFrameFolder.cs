namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Frame Folder")]
    internal class ModEmoExpressionFrameFolder : ModEmoTagComponent, IModEmoExpressionFrameProvider
    {
        protected virtual bool IncludeSelf => false;

        public IEnumerable<ExpressionFrame> GetFrames() => this.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>(includeSelf: IncludeSelf).SelectMany(x => x.GetFrames());

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in GetFrames())
            {
                hashCode.Add(x);
            }
        }
    }
}