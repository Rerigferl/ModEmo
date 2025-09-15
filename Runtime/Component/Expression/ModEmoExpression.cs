
namespace Numeira
{
    internal abstract class ModEmoExpression : ModEmoTagComponent, IModEmoExpression
    {
        public string Name = "";

        string IModEmoExpression.Name => GetName();

        ExpressionMode IModEmoExpression.Mode => GetMode();

        public virtual IEnumerable<ExpressionFrame> Frames => this.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>(includeSelf: true).SelectMany(x => x.GetFrames());

        protected virtual string GetName() => !string.IsNullOrEmpty(Name) ? Name : name;

        protected virtual ExpressionMode GetMode() => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(GetName().GetFarmHash64());
            foreach(var frame in this.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>(includeSelf: true))
            {
                frame.CalculateContentHash(ref hashCode);
            }
            foreach(var condition in this.GetComponentsInDirectChildren<IModEmoConditionProvider>(includeSelf: true))
            {
                condition.CalculateContentHash(ref hashCode);
            }
        }
    }
}