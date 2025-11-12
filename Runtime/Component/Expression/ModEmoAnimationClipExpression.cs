namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "AnimationClip Expression")]
    internal sealed class ModEmoAnimationClipExpression : ModEmoExpression, IModEmoAnimationCollector
    {
        public AnimationClip? AnimationClip;

        protected override string GetName() => string.IsNullOrEmpty(Name) && AnimationClip != null ? AnimationClip.name : base.GetName();

        void IModEmoAnimationCollector.CollectAnimation(IAnimationWriterSource source, in AnimationWriterContext context)
        {
            if (AnimationClip == null)
                return;

#if UNITY_EDITOR
            var binds = AnimationUtility.GetCurveBindings(AnimationClip);
            Dictionary<float, List<BlendShape>> dict = new();
            foreach (var bind in binds)
            {
                var propertyName = bind.propertyName;
                var curve = AnimationUtility.GetEditorCurve(AnimationClip, bind);
                if (curve.length == 0)
                    continue;

                foreach(var key in curve.keys)
                {
                    source.Write(bind, key.time, key.value);
                }
            }
#endif
        }
    }
}