
namespace Numeira
{
    internal abstract class ModEmoExpression : ModEmoNamedTagComponent, IModEmoExpression, IPreviewable, IInspectorExtension
    {
        ExpressionMode IModEmoExpression.Mode => GetMode();

        public virtual int LayerIndex => 0;

        protected virtual ExpressionMode GetMode() => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(GetName().GetFarmHash64());
            foreach (var frame in this.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(includeSelf: true))
            {
                frame.CalculateContentHash(ref hashCode);
            }
            foreach (var condition in this.GetComponentsInDirectChildren<IModEmoConditionProvider>(includeSelf: true))
            {
                condition.CalculateContentHash(ref hashCode);
            }
        }

#if UNITY_EDITOR
        public virtual void OnInspectorExtension(IInspectorExtensionContext context)
        {
            bool flag = true;
            var expression = (this as IModEmoExpression);
            using (context.BeginGroup("Expression Settings", ref flag))
            {
                using (context.BeginHorizontal())
                {
                    context.CheckBox("Loop", expression.IsLoop, value => expression.Component.GetOrAddComponent<ModEmoLoopControl>(x => x.WithVisibile(false)).enabled = value);
                    context.CheckBox("Blink", expression.Blink == true, value => expression.Component.GetOrAddComponent<ModEmoBlinkControl>(x => x.WithVisibile(false)).enabled = value);
                    context.CheckBox("LipSync", expression.LipSync, value => expression.Component.GetOrAddComponent<ModEmoLipSyncControl>(x => x.WithVisibile(false)).enabled = value);
                }

                using (context.BeginHorizontal())
                {
                    context.CheckBox("EyeTracking", expression.EyeTracking, value => expression.Component.GetOrAddComponent<ModEmoEyeTrackingControl>(x => x.WithVisibile(false)).enabled = value);
                    context.CheckBox("Mouth Morph Cancel", expression.EnableMouthMorphCancel, value => expression.Component.GetOrAddComponent<ModEmoMouthMorphCancelControl>(x => x.WithVisibile(false)).enabled = value);
                    context.CheckBox("- - - - -", false);
                }
            }
            context.RegisterComponentShortcut("Add Blendshape", go =>
            {
                Undo.AddComponent<ModEmoBlendShapeSelector>(go);
            });
        }
#endif
    }
}