namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Folder")]
    internal class ModEmoExpressionFolder : ModEmoTagComponent, IModEmoExpressionFolder
    {
        IEnumerable<IModEmoExpression> IModEmoExpressionFolder.Expressions
        {
            get
            {
                foreach (var x in this.GetComponentsInDirectChildren<IModEmoExpression>())
                    yield return x;

                foreach (var x in this.GetComponentsInDirectChildren<IModEmoExpressionFolder>())
                    foreach (var y in x.Expressions)
                        yield return y;
            }
        }
    }

    internal interface IModEmoExpressionFolder
    {
        IEnumerable<IModEmoExpression> Expressions { get; }
    }
}