namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    internal class ModEmoExpression : ModEmoTagComponent, IModEmoExpression
    {
        public string Name = "";
        public bool DesyncWithObjectName = false;
        public ExpressionMode Mode;

        [SerializeReference]
        public IModEmoExpressionCondition Condition = new VRChatExpressionCondition();

        IModEmoExpressionCondition IModEmoExpression.Condition => Condition;

        string IModEmoExpression.Name => GetName();

        IEnumerable<IModEmoExpressionFrame> IModEmoExpression.Frames => GetFrames();

        ExpressionMode IModEmoExpression.Mode => Mode;

        protected virtual string GetName() => DesyncWithObjectName ? Name : name;

        protected virtual IEnumerable<IModEmoExpressionFrame> GetFrames()
        {
            foreach(var selector in gameObject.GetComponentsInDirectChildren<ModEmoBlendShapesSelector>())
            {
                yield return new ExpressionFrame(0, selector.BlendShapes);
            }
        }
    }

    internal interface IModEmoExpression
    {
        string Name { get; }

        ExpressionMode Mode { get; }

        IModEmoExpressionCondition Condition { get; }

        IEnumerable<IModEmoExpressionFrame> Frames { get; }
    }

    internal enum ExpressionMode
    {
        Default,
        Combine,
    }
}