namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    [ExecuteInEditMode]
    internal class ModEmoDefaultExpression : ModEmoExpression
    {
        public ExpressionMode Mode;
        protected override ExpressionMode GetMode() => Mode;
    }

    [ExecuteInEditMode]
    internal abstract class ModEmoExpression : ModEmoTagComponent, IModEmoExpression
    {
        public string Name = "";
        public bool DesyncWithObjectName = false;

        string IModEmoExpression.Name => GetName();

        ExpressionMode IModEmoExpression.Mode => GetMode();

        IEnumerable<IModEmoExpressionFrame> IModEmoExpression.Frames => GetFrames();

        protected virtual string GetName() => DesyncWithObjectName ? Name : name;
        protected virtual ExpressionMode GetMode() => ExpressionMode.Default;
        protected virtual IEnumerable<IModEmoExpressionFrame> GetFrames()
        {
            foreach (var frame in gameObject.GetComponentsInDirectChildren<IModEmoExpressionFrame>())
            {
                yield return frame;
            }
        }
    }

    internal interface IModEmoExpression : IModEmoComponent
    {
        string Name { get; }

        ExpressionMode Mode { get; }

        IEnumerable<IModEmoExpressionFrame> Frames { get; }

        IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> Conditions => Component.GetComponentsInDirectChildren<IModEmoConditionProvider>().SelectMany(x => x);

        bool IsLoop => Component.GetComponent<IModEmoExpressionLoopControl>()?.IsLoop is true;
    }

    internal enum ExpressionMode
    {
        Default,
        Combine,
    }
}