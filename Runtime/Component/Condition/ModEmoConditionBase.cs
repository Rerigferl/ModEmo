

namespace Numeira
{
    [DisallowMultipleComponent]
    internal abstract class ModEmoConditionBase : ModEmoTagComponent, IModEmoConditionProvider
    {
        public abstract IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions();

        protected IModEmoConditionProvider[] Children => this.GetComponentsInDirectChildren<IModEmoConditionProvider>();
    }


    internal interface IModEmoConditionProvider : IModEmoComponent, IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>>
    {
        IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions();

        IEnumerator<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>>.GetEnumerator() => GetConditions().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(ModEmoConditionBase), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    internal sealed class ModEmoConditionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            switch (target)
            {
                case ModEmoCondition x:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(x.Parameters)));
                    }
                    break;
                case ModEmoGestureCondition x:
                    {
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(x.Hand)));
                        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(x.Gesture)));
                    }
                    break;
            }
        }
    }
#endif
}