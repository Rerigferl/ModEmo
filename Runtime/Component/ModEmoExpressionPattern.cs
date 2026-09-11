

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Pattern")]
    internal sealed class ModEmoExpressionPattern : ModEmoExpressionFolder, IModEmoExpressionPattern
    {
        ExpressionMode IModEmoExpression.Mode => ExpressionMode.Default;

        public int LayerIndex => 0;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add((this as IModEmoExpression).Name.GetFarmHash64());
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