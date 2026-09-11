namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape Folder")]
    internal class ModEmoBlendShapeFolder : ModEmoTagComponent, IModEmoBlendShapeFolder
    {
        protected virtual bool IncludeSelf => false;

        public bool OverrideKeyframe = false;
        public float Keyframe = 0;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
        }

        public void WriteKeyframes(IKeyframeWriterContext context, in AnimationGeneratorOptions options)
        {
            var proxy = Proxy.Instance;
            proxy.Original = context;
            proxy.Time = OverrideKeyframe ? Keyframe : null;

            foreach(var x in this.GetOwnedComponents())
            {
                x.WriteKeyframes(proxy, options);
            }
        }

        private sealed class Proxy : IKeyframeWriterContext
        {
            public static readonly Proxy Instance = new();

            public IKeyframeWriterContext? Original { get; set; }
            public float? Time { get; set; }

            public void AddBlendshape(Transform target, string name, float time, float value)
            {
                Original?.AddBlendshape(target, name, Time ?? time, value);
            }

            public void AddCancelBlendshape(Transform target, string name, float time, float value)
            {
                Original?.AddCancelBlendshape(target, name, Time ?? time, value);
            }

            public void AddRotation(Transform target, float time, Vector3 eularAngle, bool relative = true)
            {
                Original?.AddRotation(target, Time ?? time, eularAngle, relative);
            }

            public void SetAnimatorParameter<T>(string name, float time, T value)
            {
                Original?.SetAnimatorParameter(name, Time ?? time, value);
            }

            public void SetAvatarParameter<T>(string name, T value)
            {
                Original?.SetAvatarParameter(name, value);
            }
        }
    }

    internal interface IModEmoBlendShapeFolder : IModEmoComponent, IOwnerComponent<IBlendshapeWriterComponent>, IBlendshapeWriterComponent
    {
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