

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape Folder")]
    internal class ModEmoBlendShapeFolder : ModEmoTagComponent, IModEmoBlendShapeProvider, IModEmoAnimationProvider
    {
        protected virtual bool IncludeSelf => false;

        public bool OverrideKeyframe = false;
        public float Keyframe = 0;

        public IModEmoBlendShapeProvider[] Children => this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: IncludeSelf);

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in Children)
            {
                x.CalculateContentHash(ref hashCode);
            }
        }

        public void WriteAnimation(IAnimationWriter writer, in AnimationWriterContext context)
        {
            using var modifyKeyframe = OverrideKeyframe ? writer.RegisterPreWriteKeyframe((ref AnimationBinding _, ref Curve.Keyframe keyframe) => keyframe.Time = Keyframe) : default;

            foreach (var x in Children)
            {
                if (x is IModEmoAnimationProvider a)
                    a.WriteAnimation(writer, context);
            }
        }

        public IEnumerable<BlendShape> GetBlendShapes() => this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: true).SelectMany(x => x.GetBlendShapes());
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