
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    internal class ModEmoDefaultExpression : ModEmoExpression
    {
        [HideInInspector]
        public ExpressionMode Mode;
        protected override ExpressionMode GetMode() => Mode;
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(ModEmoDefaultExpression))]
    internal sealed class ModEmoDefaultExpressionEditor : ModEmoExpressionEditorBase
    {
        protected override void OnInnerInspectorGUI()
        {
            
        }
    }
#endif
}