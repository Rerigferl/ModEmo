
namespace Numeira
{
    internal abstract class ModEmoExpression : ModEmoNamedTagComponent, IModEmoExpression
    {
        ExpressionMode IModEmoExpression.Mode => GetMode();

        public virtual int LayerIndex => 0;

        protected virtual ExpressionMode GetMode() => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(GetName().GetFarmHash64());
            foreach (var frame in this.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(includeSelf: true))
            {
                frame.CalculateContentHash(ref hashCode);
            }
            foreach (var condition in this.GetComponentsInDirectChildren<IModEmoConditionProvider>(includeSelf: true))
            {
                condition.CalculateContentHash(ref hashCode);
            }
        }
    }
}