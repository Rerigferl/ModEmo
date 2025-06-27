using UnityEngine;

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "AnimationClip Expression")]
    internal sealed class ModEmoAnimationClipExpression : ModEmoDefaultExpression
    {
        public AnimationClip? AnimationClip;
        public bool NameFromAnimationClip = true;

        protected override string GetName() => NameFromAnimationClip && AnimationClip != null ? AnimationClip.name : base.GetName();

        protected override IEnumerable<IModEmoExpressionFrame> GetFrames()
        {
            if (AnimationClip == null)
                yield break;

            var binds = AnimationUtility.GetCurveBindings(AnimationClip);

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
                    yield return new ExpressionFrame(key.time, Enumerable.Repeat(new BlendShape() { Name = propertyName, Value = key.value }, 1), this);
                }
            }

            foreach (var x in base.GetFrames())
                yield return x;
        }
    }
}