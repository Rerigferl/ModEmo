namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Patterns")]
    internal sealed class ModEmoExpressionPatterns : ModEmoExpressionFolder, IModEmoExpressionPatterns
    {
        public string Name = "";
        public bool DesyncWithObjectName = false;

        [SerializeReference]
        public IModEmoExpression? DefaultExpression;

        string IModEmoExpressionPatterns.Name => DesyncWithObjectName ? Name : name;

        IModEmoExpression? IModEmoExpressionPatterns.DefaultExpression => this.DefaultExpression;
    }

    internal interface IModEmoExpressionPatterns : IModEmoExpressionFolder
    {
        string Name { get; }

        IModEmoExpression? DefaultExpression { get; }
    }
}