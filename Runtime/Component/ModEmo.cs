using System.Collections.Immutable;
using nadena.dev.modular_avatar.core;

namespace Numeira
{
    [AddComponentMenu("ModEmo/ModEmo")]
    internal sealed class ModEmo : ModEmoTagComponent
    {
        public ModEmoSettings Settings = new();

        public ModEmoExpression? BlinkExpression;

        public IEnumerable<IGrouping<IModEmoExpressionPatterns, IModEmoExpression>> ExportExpressions()
            => new ExpressionGroups(this);

        private sealed class ExpressionGroups : IEnumerable<ExpressionGroup>
        {
            private readonly ModEmo root;

            public ExpressionGroups(ModEmo root)
            {
                this.root = root;
            }

            public IEnumerator<ExpressionGroup> GetEnumerator()
            {
                foreach(var x in root.gameObject)
                {
                    if (x.GetComponent<IModEmoExpressionPatterns>() != null)
                        yield return new(x);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ExpressionGroup : IGrouping<IModEmoExpressionPatterns, IModEmoExpression>
        {
            private readonly GameObject expressionPatterns;

            public ExpressionGroup(GameObject expressionPatterns)
            {
                this.expressionPatterns = expressionPatterns;
            }

            public IModEmoExpressionPatterns Key => expressionPatterns.GetComponent<IModEmoExpressionPatterns>();

            public IEnumerator<IModEmoExpression> GetEnumerator() => Key.Expressions.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }

    [Serializable]
    internal sealed class ModEmoSettings
    {
        public AvatarObjectReference Face = new() { referencePath = "Body" };

        public string SeparatorStringRegEx = /* lang=regex */ "[-=]{2,}";
    }
}