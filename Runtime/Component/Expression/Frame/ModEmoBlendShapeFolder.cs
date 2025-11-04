
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape Folder")]
    internal class ModEmoBlendShapeFolder : ModEmoTagComponent, IModEmoBlendShapeProvider
    {
        protected virtual bool IncludeSelf => false;

        public bool OverrideKeyframe = false;
        public float Keyframe = 0;

        public IModEmoBlendShapeProvider[] Children => this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: IncludeSelf);

        public void CollectBlendShapes(in BlendShapeCurveWriter writer)
        {
            if (OverrideKeyframe)
            {
                writer.BeginModifyCurveTime(_ => Keyframe);
            }

            foreach (var x in Children)
            {
                x.CollectBlendShapes(writer);
            }

            if (OverrideKeyframe)
            {
                writer.EndModifyCurveTime();
            }
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in Children)
            {
                x.CalculateContentHash(ref hashCode);
            }
        }
    }

#if UNITY_EDITOR

    static partial class RuntimeEditor
    {
        [CustomEditor(typeof(ModEmoBlendShapeFolder))]
        internal sealed class ModEmoBlendShapeFolderEditor : Editor
        {
            private SerializedProperty OverrideKeyframeProperty = null!;
            private SerializedProperty KeyframeProperty = null!;

            public void OnEnable()
            {
                OverrideKeyframeProperty = serializedObject.FindProperty(nameof(ModEmoBlendShapeFolder.OverrideKeyframe));
                KeyframeProperty = serializedObject.FindProperty(nameof(ModEmoBlendShapeFolder.Keyframe));
            }

            public override void OnInspectorGUI()
            {
                serializedObject.Update();

                EditorGUILayout.PropertyField(OverrideKeyframeProperty);
                EditorGUI.BeginDisabledGroup(!OverrideKeyframeProperty.boolValue);
                EditorGUILayout.PropertyField(KeyframeProperty);
                EditorGUI.EndDisabledGroup();

                serializedObject.ApplyModifiedProperties();
            }
        }
    }

#endif
}