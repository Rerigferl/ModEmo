namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Patterns")]
    internal sealed class ModEmoExpressionPatterns : ModEmoTagComponent, IModEmoExpressionPatterns
    {
        public string Name = "";
        public bool DesyncWithObjectName = false;

        public ModEmoExpression? DefaultExpression;

        string IModEmoExpressionPatterns.Name => DesyncWithObjectName ? Name : name;

        IEnumerable<IModEmoExpression> IModEmoExpressionPatterns.Expressions => this.GetComponentsInDirectChildren<IModEmoExpression>();
    }

    internal interface IModEmoExpressionPatterns
    {
        string Name { get; }

        IEnumerable<IModEmoExpression> Expressions { get; }
    }
}