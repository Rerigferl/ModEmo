namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    internal class ModEmoExpression : ModEmoExpressionBase
    {
        public ExpressionMode Mode;
        public Hand UseHandWeight;
        [SerializeReference]
        public IModEmoExpressionCondition Condition = new VRChatExpressionCondition();

        protected override IModEmoExpressionCondition GetCondition() => Condition;
        protected override ExpressionMode GetMode() => Mode;
        protected override Hand GetUseGestureWeight() => UseHandWeight;
    }

    internal abstract class ModEmoExpressionBase : ModEmoTagComponent, IModEmoExpression
    {
        public string Name = "";
        public bool DesyncWithObjectName = false;

        string IModEmoExpression.Name => GetName();

        ExpressionMode IModEmoExpression.Mode => GetMode();

        Hand IModEmoExpression.UseGestureWeight => GetUseGestureWeight();

        IModEmoExpressionCondition IModEmoExpression.Condition => GetCondition();

        IEnumerable<IModEmoExpressionFrame> IModEmoExpression.Frames => GetFrames();

        protected virtual string GetName() => DesyncWithObjectName ? Name : name;
        protected virtual ExpressionMode GetMode() => ExpressionMode.Default;
        protected virtual Hand GetUseGestureWeight() => default;
        protected virtual IModEmoExpressionCondition GetCondition() => throw new NotImplementedException();
        protected virtual IEnumerable<IModEmoExpressionFrame> GetFrames()
        {
            foreach (var selector in gameObject.GetComponentsInDirectChildren<ModEmoBlendShapesSelector>())
            {
                yield return new ExpressionFrame(0, selector.BlendShapes);
            }
            foreach (var frame in gameObject.GetComponentsInDirectChildren<IModEmoExpressionFrame>())
            {
                yield return frame;
            }
        }
    }

    internal interface IModEmoExpression
    {
        string Name { get; }

        ExpressionMode Mode { get; }

        Hand UseGestureWeight { get; }

        IModEmoExpressionCondition Condition { get; }

        IEnumerable<IModEmoExpressionFrame> Frames { get; }
    }

    internal enum ExpressionMode
    {
        Default,
        Combine,
    }
}