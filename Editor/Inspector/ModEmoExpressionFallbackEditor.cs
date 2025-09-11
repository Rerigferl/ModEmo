namespace Numeira;


[CustomEditor(typeof(ModEmoExpression), true, isFallback = true)]
internal sealed class ModEmoExpressionFallbackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.Slider(ExpressionPreview.PreviewTime, 0, 1);
        EditorGUILayout.Separator();

        base.OnInspectorGUI();
    }
}