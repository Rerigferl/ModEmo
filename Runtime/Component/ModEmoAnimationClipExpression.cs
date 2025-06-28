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
                yield return new ExpressionFrame(key, blendshapes, this);
            }

            foreach (var x in base.GetFrames())
                yield return x;
        }
    }
}