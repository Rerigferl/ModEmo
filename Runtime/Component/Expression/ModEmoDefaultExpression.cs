
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    internal class ModEmoDefaultExpression : ModEmoExpression, IModEmoExpression
    {
        [HideInInspector]
        public ExpressionMode Mode;
        protected override ExpressionMode GetMode() => Mode;
    }
}