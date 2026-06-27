

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Pattern")]
    internal sealed class ModEmoExpressionPattern : ModEmoExpressionFolder, IModEmoExpressionPattern
    {
        public IEnumerable<BlendShape> GetBlendShapes() => this.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(includeSelf: true).Where(x => !x.GameObject.GetComponents<IModEmoExpression>().Where(x => x is not IModEmoExpressionPattern).Any()).SelectMany(x => x.GetBlendShapes());

        ExpressionMode IModEmoExpression.Mode => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add((this as IModEmoExpression).Name.GetFarmHash64());
            foreach (var x in this.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(includeSelf: true))
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