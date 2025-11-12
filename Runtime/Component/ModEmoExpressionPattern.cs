

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Pattern")]
    internal sealed class ModEmoExpressionPattern : ModEmoExpressionFolder, IModEmoExpressionPattern, IModEmoComponent
    {
        public string Name = "";

        public IEnumerable<BlendShape> GetBlendShapes() => this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: true).Where(x => !x.GameObject.GetComponents<IModEmoExpression>().Where(x => x is not IModEmoExpressionPattern).Any()).SelectMany(x => x.GetBlendShapes());

        string IModEmoExpression.Name => !string.IsNullOrEmpty(Name) ? Name : name;

        ExpressionMode IModEmoExpression.Mode => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add((this as IModEmoExpression).Name.GetFarmHash64());
            foreach (var x in this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: true))
            {
                x.CalculateContentHash(ref hashCode);
            }
            base.CalculateContentHash(ref hashCode);
        }
    }

    internal interface IModEmoExpressionPattern : IModEmoExpression, IModEmoExpressionFolder
    {
    }

#if UNITY_EDITOR
    static partial class RuntimeEditor
    {
        [CustomEditor(typeof(ModEmoExpressionPattern))]
        internal sealed class ModEmoExpressionPatternEditor : ModEmoComponentEditorBase
        {
        }
    }
#endif
}