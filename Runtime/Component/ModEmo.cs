using System.Runtime.Remoting.Contexts;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf.runtime;

namespace Numeira
{
    [AddComponentMenu("ModEmo/ModEmo")]
    internal sealed class ModEmo : ModEmoTagComponent
    {
        public ModEmoSettings Settings = new();

        public IEnumerable<IGrouping<IModEmoExpressionPattern, IModEmoExpression>> ExportExpressions()
            => new ExpressionGroups(this);

        public IModEmoExpressionPattern[] Patterns => this.GetComponentsInDirectChildren<IModEmoExpressionPattern>();

        public IModEmoExpression? GetBlinkExpression() => this.GetComponentsInDirectChildren<IModEmoExpression>().Where(x => x is not IModEmoExpressionPattern).FirstOrDefault();

        public IModEmoMouthMorphCanceller? MouthMorphCanceller => this.GetComponentInDirectChildren<IModEmoMouthMorphCanceller>();

        private sealed class ExpressionGroups : IEnumerable<ExpressionGroup>
        {
            private readonly ModEmo root;

            public ExpressionGroups(ModEmo root)
            {
                this.root = root;
            }

            public IEnumerator<ExpressionGroup> GetEnumerator()
            {
                foreach (var x in root.gameObject)
                {
                    if (!x.activeInHierarchy)
                        continue;
                    if (x.GetComponent<IModEmoExpressionPattern>() != null)
                        yield return new(x);
                }
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private sealed class ExpressionGroup : IGrouping<IModEmoExpressionPattern, IModEmoExpression>
        {
            private readonly GameObject expressionPatterns;

            public ExpressionGroup(GameObject expressionPatterns)
            {
                this.expressionPatterns = expressionPatterns;
            }

            public IModEmoExpressionPattern Key => expressionPatterns.GetComponent<IModEmoExpressionPattern>();

            public IEnumerator<IModEmoExpression> GetEnumerator() => Key.Expressions.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public SkinnedMeshRenderer? GetFaceRenderer()
        {
            var avatarRoot = RuntimeUtil.FindAvatarInParents(transform);
            if (avatarRoot == null)
                return null;

            return avatarRoot.GetComponentInChildren<ModEmoFaceObject>()?.Renderer ?? avatarRoot.Find("Body")?.GetComponent<SkinnedMeshRenderer>();
        }

        public override int GetHashCode()
        {
            HashCode hash = new();
            CalculateContentHash(ref hash);
            return hash.ToHashCode();
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in this.GetComponentsInDirectChildren<IModEmoExpressionPattern>())
            {
                x.CalculateContentHash(ref hashCode);
            }
        }
    }

    [Serializable]
    internal sealed class ModEmoSettings
    {
        // lang=regex 
        public string SeparatorStringRegEx = "[-=]{2,}";

        // Broken...
        [HideInInspector]
        public bool UseCache = false;

        [Range(0, 1)]
        public float SmoothFactor = 0.85f;

        public ModEmoDebugSettings DebugSettings = new();
    }

    [Serializable]
    internal sealed class ModEmoDebugSettings
    {
        public bool SkipExpressionController = false;
    }
}