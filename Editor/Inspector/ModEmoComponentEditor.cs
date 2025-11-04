
namespace Numeira;

[CustomEditor(typeof(ModEmo))]
internal sealed class ModEmoComponentEditor : Editor
{
    private ModEmo Target => (target as ModEmo)!;
    private IModEmoExpression? BlinkExpression;

    private IModEmoExpressionPattern[] Patterns = null!;

    public void OnEnable()
    {
        BlinkExpression = Target.GetBlinkExpression();
        Patterns = Target.Patterns;
    }

    public void OnDisable()
    {
    }

    public override void OnInspectorGUI()
    {
        DrawHeader();

        serializedObject.Update();

        {
            var rect = (GUIPosition)EditorGUILayout.GetControlRect(true);
            rect.ObjectField("Blink", BlinkExpression?.Component, objectType: typeof(ModEmoExpression), readOnly: true);
        }
        EditorGUILayout.Space();
        {
            EditorGUILayout.BeginFoldoutHeaderGroup(true, "Patterns");
            foreach (var x in Patterns)
            {
                var rect = (GUIPosition)EditorGUILayout.GetControlRect(true);
                rect.ObjectField("", x.Component, objectType: typeof(ModEmoExpressionPattern), readOnly: true);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.Space();
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(ModEmo.Settings)));
        serializedObject.ApplyModifiedProperties();
    }

    protected override void OnHeaderGUI()
    {

    }

    private sealed class PatternList : ReorderableListWrapper
    {
        public PatternList(SerializedObject serializedObject, SerializedProperty elements) : base(serializedObject, elements, "Patterns")
        {
        }

        protected override void OnItemGUI(Rect rect, int index, bool isActive, bool isFocused)
        {

        }
    }
}