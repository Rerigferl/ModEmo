namespace Numeira
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    internal sealed class ModEmoExpressionMotionFolder : ModEmoTagComponent 
    {
        public static ModEmoExpressionMotionFolder New(Transform parent)
        {
            var newObj = new GameObject("Motion");
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(newObj, "Add Motion Folder");
            EditorUtility.SetDirty(parent.gameObject);
#endif
            newObj.transform.parent = parent;
            return newObj.AddComponent<ModEmoExpressionMotionFolder>();
        }
    }
}