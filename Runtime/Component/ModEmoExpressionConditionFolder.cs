namespace Numeira
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    internal sealed class ModEmoExpressionConditionFolder : ModEmoTagComponent
    {
        public static ModEmoExpressionConditionFolder New(Transform parent)
        {
            var newObj = new GameObject("Condition");
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(newObj, "Add Condition Folder");
            EditorUtility.SetDirty(parent.gameObject);
#endif
            newObj.transform.parent = parent;
            return newObj.AddComponent<ModEmoExpressionConditionFolder>();
        }

        public IModEmoCondition? GetCondition() => gameObject.GetComponentsInDirectChildren<IModEmoCondition>().FirstOrDefault();
    }
}