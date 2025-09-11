
namespace Numeira
{
    [DisallowMultipleComponent]
    [AddComponentMenu(ComponentMenuPrefix + "Condition Folder")]
    internal sealed class ModEmoConditionFolder : ModEmoTagComponent, IModEmoConditionProvider
    {
        public static ModEmoConditionFolder New(Transform parent)
        {
            var newObj = new GameObject("Condition");
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(newObj, "Add Condition Folder");
            EditorUtility.SetDirty(parent.gameObject);
#endif
            newObj.transform.parent = parent;
            return newObj.AddComponent<ModEmoConditionFolder>();
        }

        public IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions() 
            => gameObject.GetComponentsInDirectChildren<IModEmoConditionProvider>().SelectMany(x => x);

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach(var child in gameObject.GetComponentsInDirectChildren<IModEmoConditionProvider>())
            {
                child.CalculateContentHash(ref hashCode);
            }
        }
    }
}