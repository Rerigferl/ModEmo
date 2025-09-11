namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Folder")]
    internal class ModEmoExpressionFolder : ModEmoTagComponent, IModEmoExpressionFolder
    {
        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in (this as IModEmoExpressionFolder).Expressions)
            {
                x.CalculateContentHash(ref hashCode);
            }
        }
    }

    internal interface IModEmoExpressionFolder : IModEmoComponent
    {
        IEnumerable<IModEmoExpression> Expressions
        {
            get
            {
                foreach (var x in Component.GetComponentsInDirectChildren<IModEmoExpression>())
                {
                    if (x == this)
                        continue;

                    yield return x;
                }

                foreach (var x in Component.GetComponentsInDirectChildren<IModEmoExpressionFolder>())
                {
                    if (x == this)
                        continue;

                    foreach (var y in x.Expressions)
                    {
                        yield return y;
                    }
                }
            }
        }
    }
}