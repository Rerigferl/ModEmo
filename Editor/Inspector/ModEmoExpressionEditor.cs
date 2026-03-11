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

            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Loop", Target.IsLoop), value => Target.Component.GetOrAddComponent<ModEmoLoopControl>(x => x.WithVisibile(false)).enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Blink", Target.Blink == true), value => Target.Component.GetOrAddComponent<ModEmoBlinkControl>(x => x.WithVisibile(false)).enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("LipSync", Target.LipSync), value => Target.Component.GetOrAddComponent<ModEmoLipSyncControl>(x => x.WithVisibile(false)).enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("EyeTracking", Target.EyeTracking), value => Target.Component.GetOrAddComponent<ModEmoEyeTrackingControl>(x => x.WithVisibile(false)).enabled = value);
            RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle("Mouth Morph Canceller", Target.EnableMouthMorphCancel), value => Target.Component.GetOrAddComponent<ModEmoMouthMorphCancelControl>(x => x.WithVisibile(false)).enabled = value);

            if (GUILayout.Button("Toggle Visibility"))
            {
                var components = Target.Component.GetComponents<IModEmoExpressionControl>();
                var c = components.FirstOrDefault().Visible;
                foreach(var x in components)
                {
                    x.Visible = !c;
                }
                Repaint();
            }

            EditorGUILayout.Space();

#if VRC_SDK_VRCSDK3

            RuntimeGUIUtils.ChangeCheck(
                () => RuntimeGUIUtils.NullableField(Target.Component.GetComponent<ModEmoGestureWeightMotionTime>()?.Side, value => (Hand)EditorGUILayout.EnumPopup("Gesture Weight", value)), 
                value => Target.Component.GetOrAddComponent<ModEmoGestureWeightMotionTime>().Side = value);

            EditorGUILayout.Space();
#endif
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