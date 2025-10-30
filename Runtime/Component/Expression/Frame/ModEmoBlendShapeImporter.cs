

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "BlendShape Import")]
    internal sealed class ModEmoBlendShapeImporter : ModEmoTagComponent, IModEmoExpressionFrameProvider
    {
        public AnimationClip? AnimationClip;

        public IEnumerable<ExpressionFrame> GetFrames()
        {
            if (AnimationClip == null)
                yield break;

            yield return ExpressionFrame.Create(this, 0, GetBlendShapes(AnimationClip));
        }

        private static IEnumerable<BlendShape> GetBlendShapes(AnimationClip animationClip)
        {
            #if UNITY_EDITOR
            var bindings = AnimationUtility.GetCurveBindings(animationClip);
            foreach (var binding in bindings)
            {
                if (binding.type != typeof(SkinnedMeshRenderer) || !binding.propertyName.StartsWith("blendshape.", StringComparison.OrdinalIgnoreCase))
                    continue;

                var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                float value = curve.Evaluate(0);

                yield return new BlendShape() { Name = binding.propertyName["blendShape.".Length..], Value = value };
            }
            #else
            yield break;
            #endif
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(AnimationClip);
        }
    }
}