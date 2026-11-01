namespace Numeira
{
    internal sealed class ModEmoFaceTrackingExpression : ModEmoExpression, IAnimationSourceComponent, IOwnerComponent<IFaceTrackingExpressionMarker>
    {
        public bool AllowSelf => false;

        void IAnimationSourceComponent.RegisterAnimations(IAnimationRegistry registry, in AnimationGeneratorOptions options)
        {
            // 
        }

        public IModEmoExpression? FindExpression(FaceTrackingMarkerRole role)
        {
            var components = this.GetOwnedComponents<IFaceTrackingExpressionMarker>();
            foreach (var component in components)
            {
                if (component.Role.HasFlag(role))
                    return component.Expression;
            }

            return null;
        }
#if UNITY_EDITOR
        public override void OnInspectorExtension(IInspectorExtensionContext context)
        {
            base.OnInspectorExtension(context);

            context.RegisterComponentShortcut("Add Expression", go =>
            {
                var go2 = new GameObject("New Expression");
                Undo.RegisterCreatedObjectUndo(go2, go2.name);
                go2.AddComponent<FaceTrackingExpressionMarker>();
                go2.AddComponent<ModEmoDefaultExpression>();
                RuntimeEditor.CreateNewObject?.Invoke(go2, go, false);
            });
        }
#endif
    }

#if UNITY_EDITOR

    static partial class RuntimeEditor
    {
        [CustomEditor(typeof(ModEmoFaceTrackingExpression))]
        public sealed class ModEmoFaceTrackingExpressionEditor : Editor
        {
            private bool foldout_Left = false;
            private bool foldout_Right = false;

            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                var c = target as ModEmoFaceTrackingExpression;
                if (c == null)
                    return;

                if (FoldOut("Left", ref foldout_Left))
                {
                    ShowTargetObjects(c, FaceTrackingMarkerRole.Left);
                }

                if (FoldOut("Right", ref foldout_Right))
                {
                    ShowTargetObjects(c, FaceTrackingMarkerRole.Right);
                }

                static bool FoldOut(string label, ref bool flag)
                {
                    flag = EditorGUILayout.BeginFoldoutHeaderGroup(flag, label);
                    EditorGUILayout.EndFoldoutHeaderGroup();
                    return flag;
                }

                static void ShowTargetObjects(ModEmoFaceTrackingExpression c, FaceTrackingMarkerRole side)
                {
                    EditorGUILayout.ObjectField("Close", c.FindExpression(side | FaceTrackingMarkerRole.Close)?.GameObject, typeof(GameObject), true);
                    EditorGUILayout.ObjectField("Look Left - Right", c.FindExpression(side | FaceTrackingMarkerRole.LookX)?.GameObject, typeof(GameObject), true);
                    EditorGUILayout.ObjectField("Look Down - Up", c.FindExpression(side | FaceTrackingMarkerRole.LookY)?.GameObject, typeof(GameObject), true);
                }
            }
        }
    }

#endif
}