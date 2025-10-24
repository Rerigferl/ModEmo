namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "AnimationClip Expression")]
    internal sealed class ModEmoAnimationClipExpression : ModEmoExpression
    {
        public AnimationClip? AnimationClip;

        protected override string GetName() => string.IsNullOrEmpty(Name) && AnimationClip != null ? AnimationClip.name : base.GetName();

        private IEnumerable<ExpressionFrame> GetFrames()
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

                foreach(var key in curve.keys)
                {
                    dict.GetOrAdd(key.time, _ => new()).Add(new BlendShape() { Name = propertyName, Value = key.value });
                }
            }

            foreach(var (key, blendshapes) in dict)
            {
                yield return ExpressionFrame.Create(this, key, blendshapes);
            }
#endif
        }

        public override IEnumerable<ExpressionFrame> Frames => GetFrames().Concat(base.Frames);
    }
}