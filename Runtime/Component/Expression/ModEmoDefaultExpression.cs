namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    [ExecuteInEditMode]
    internal class ModEmoDefaultExpression : ModEmoExpression
    {
        [HideInInspector]
        public ExpressionMode Mode;
        protected override ExpressionMode GetMode() => Mode;
    }
}