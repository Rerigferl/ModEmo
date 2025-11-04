namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "AnimationClip Expression")]
    internal sealed class ModEmoAnimationClipExpression : ModEmoExpression
    {
        public AnimationClip? AnimationClip;

        protected override string GetName() => string.IsNullOrEmpty(Name) && AnimationClip != null ? AnimationClip.name : base.GetName();

        public override IEnumerable<CurveBlendShape> BlendShapes
        {
            get
            {
                if (AnimationClip == null)
                    yield break;
#if UNITY_EDITOR
                var binds = AnimationUtility.GetCurveBindings(AnimationClip);
                Dictionary<float, List<BlendShape>> dict = new();
                foreach (var bind in binds)
                {
                    var propertyName = bind.propertyName;
                    if (!propertyName.StartsWith("blendShape."))
                        continue;
                    propertyName = propertyName["blendShape.".Length..];
                    var curve = AnimationUtility.GetEditorCurve(AnimationClip, bind);
                    if (curve.length == 0)
                        continue;

                    yield return new CurveBlendShape(propertyName, (Curve)curve);
                }
#endif
                yield break;
            }
        }
    }
}