namespace Numeira;

internal static partial class RuntimeEditor
{
    public static Action<GameObject, GameObject>? CreateNewObject;

    internal abstract class ModEmoFolderComponentEditorBase<T> : Editor where T : IModEmoComponent
    {

    }

    internal abstract class ModEmoComponentEditorBase : Editor
    {
        private SerializedProperty? nameProperty;

        protected IModEmoComponent Target => (target as IModEmoComponent)!;

        public virtual void OnEnable()
        {
            nameProperty = serializedObject.FindProperty("Name");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            OnInnerInspectorGUI();
            serializedObject.ApplyModifiedProperties();
        }

        public virtual void OnInnerInspectorGUI()
        {
            if (nameProperty != null)
            {
                var rect = EditorGUILayout.GetControlRect();
                ((GUIPosition)rect).TextField("Name", nameProperty, target.name);
            }
        }
    }
}
