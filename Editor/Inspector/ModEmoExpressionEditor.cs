namespace Numeira;


[CustomEditor(typeof(ModEmoExpression), true)]
internal class ModEmoExpressionEditor : Editor
{
    public IModEmoExpression Target => (target as IModEmoExpression)!;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        ((GUIPosition)EditorGUILayout.GetControlRect()).TextField("Name", GetNameProperty(serializedObject), Target.Name);

        OnInnerInspectorGUI();

        serializedObject.ApplyModifiedProperties();


        if (Target is not ModEmoBlinkExpression)
        {
            EditorGUILayout.Space();

            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Loop", Target.IsLoop), value => Target.Component.GetOrAddComponent<ModEmoLoopControl>().enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Blink", Target.Blink == true), value => Target.Component.GetOrAddComponent<ModEmoBlinkControl>().enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("LipSync", Target.LipSync), value => Target.Component.GetOrAddComponent<ModEmoLipSyncControl>().enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Mouth Morph Canceller", Target.EnableMouthMorphCancel), value => Target.Component.GetOrAddComponent<ModEmoMouthMorphCancelControl>().enabled = value);

            EditorGUILayout.Space();
        }
    }

    protected virtual SerializedProperty GetNameProperty(SerializedObject so) => so.FindProperty("Name");

    protected virtual void OnInnerInspectorGUI() { }
}

[CustomEditor(typeof(ModEmoAnimationClipExpression))]
internal sealed class ModEmoAnimationClipExpressionEditor : ModEmoExpressionEditor
{
    protected override void OnInnerInspectorGUI()
    {
        EditorGUILayout.ObjectField(serializedObject.FindProperty("AnimationClip"), typeof(AnimationClip));
    }
}