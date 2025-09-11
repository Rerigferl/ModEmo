
namespace Numeira
{
    internal abstract class ModEmoExpression : ModEmoTagComponent, IModEmoExpression
    {
        public string Name = "";

        string IModEmoExpression.Name => GetName();

        ExpressionMode IModEmoExpression.Mode => GetMode();

        public virtual IEnumerable<ExpressionFrame> Frames => gameObject.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>().SelectMany(x => x.GetFrames());

        protected virtual string GetName() => !string.IsNullOrEmpty(Name) ? Name : name;

        protected virtual ExpressionMode GetMode() => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(GetName().GetFarmHash64());
            foreach(var frame in gameObject.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>())
            {
                frame.CalculateContentHash(ref hashCode);
            }
            foreach(var condition in gameObject.GetComponentsInDirectChildren<IModEmoConditionProvider>())
            {
                condition.CalculateContentHash(ref hashCode);
            }
        }
    }
}