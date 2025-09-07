
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Patterns")]
    internal sealed class ModEmoExpressionPatterns : ModEmoExpressionFolder, IModEmoExpressionPatterns, IModEmoComponent
    {
        public string Name = "";

        public ModEmoExpression? DefaultExpression;

        string IModEmoExpressionPatterns.Name => !string.IsNullOrEmpty(Name) ? Name : name;

        IModEmoExpression? IModEmoExpressionPatterns.DefaultExpression => this.DefaultExpression;
    }

    internal interface IModEmoExpressionPatterns : IModEmoExpressionFolder
    {
        string Name { get; }

        IModEmoExpression? DefaultExpression { get; }
    }
}