namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Folder")]
    internal class ModEmoExpressionFolder : ModEmoNamedTagComponent, IModEmoExpressionFolder
    {
        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in (this as IModEmoExpressionFolder).Expressions)
            {
                x.CalculateContentHash(ref hashCode);
            }
        }
    }

    internal interface IModEmoExpressionFolder : IModEmoNamedComponent, ISubComponent<IModEmoExpressionFolder>, IOwnerComponent<IModEmoExpression>, IOwnerComponent<IModEmoExpressionFolder>
    {
        IEnumerable<IModEmoExpression> Expressions
        {
            get
            {
                foreach(var x in this.GetOwnedComponents<IModEmoExpression>())
                {
                    yield return x;
                }
                foreach (var x in this.GetOwnedComponents<IModEmoExpressionFolder>())
                {
                    foreach (var y in x.Expressions)
                        yield return y;
                }
            }
        }
    }
}