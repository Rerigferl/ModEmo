namespace Numeira
{
    internal interface IModEmoExpressionFrameProvider : IModEmoComponent, IEnumerable<ExpressionFrame>
    {
        public IEnumerable<ExpressionFrame> GetFrames();

        IEnumerator<ExpressionFrame> IEnumerable<ExpressionFrame>.GetEnumerator() => GetFrames().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetFrames().GetEnumerator();
    }
}