namespace Numeira
{
    internal interface IModEmoExpression : IModEmoComponent
    {
        string Name { get; }

        ExpressionMode Mode { get; }

        IEnumerable<ExpressionFrame> Frames { get; }

        IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> Conditions => Component.GetComponentsInDirectChildren<IModEmoConditionProvider>(includeSelf: true).SelectMany(x => x);

        bool IsLoop => Component.GetComponent<IModEmoLoopControl>()?.IsLoop is true;

        string? MotionTime => Component.GetComponent<IModEmoMotionTimeProvider>()?.ParameterName;

        bool Blink => Component.GetComponent<IModEmoBlinkControl>()?.Enable ?? true;

        bool LipSync => Component.GetComponent<IModEmoLipSyncConttrol>()?.Enable ?? true;

        bool EnableMouthMorphCancel => Component.GetComponent<IModEmoMouthMorphCancelControl>()?.Enable ?? false;
    }

#if UNITY_EDITOR
    internal abstract class ModEmoExpressionEditorBase : Editor
    {
        public static Action<GameObject, GameObject>? CreateNewObject;

        public IModEmoExpression Target => (target as IModEmoExpression)!;
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            ((GUIPosition)EditorGUILayout.GetControlRect()).TextField("Name", GetNameProperty(serializedObject), Target.Name);

            OnInnerInspectorGUI();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Loop", Target.IsLoop), value => Target.Component.GetOrAddComponent<ModEmoLoopControl>().enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Blink", Target.Blink == true), value => Target.Component.GetOrAddComponent<ModEmoBlinkControl>().enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("LipSync", Target.LipSync), value => Target.Component.GetOrAddComponent<ModEmoLipSyncControl>().enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Mouth Morph Canceller", Target.EnableMouthMorphCancel), value => Target.Component.GetOrAddComponent<ModEmoMouthMorphCancelControl>().enabled = value);


            EditorGUILayout.Space();

        }

        protected virtual SerializedProperty GetNameProperty(SerializedObject so) => so.FindProperty("Name");

        protected abstract void OnInnerInspectorGUI();
    }
#endif
}