
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "EyeTracking Blink Expression")]
    internal sealed class ModEmoEyetrackingBlinkExpression : ModEmoExpression, IModEmoExpression, IModEmoLoopControl, IModEmoAnimationCollector
    {
        public bool IsLoop => true;
        public override int LayerIndex => 1;

        IEnumerable<string> IModEmoExpression.MotionTime
        {
            get
            {
                yield return "/tracking/eye/EyesClosedAmount";
            }
        }

        public IEnumerable<BlendShape> GetUsedBlendshapes() => this.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(includeSelf: true).SelectMany(x => x.GetUsedBlendshapes());

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var b in GetUsedBlendshapes())
                hashCode.Add(b);

            base.CalculateContentHash(ref hashCode);
        }

        void IModEmoAnimationCollector.CollectAnimation(IAnimationWriterSource source, in AnimationWriterContext context)
        {
            var children = (this as IModEmoAnimationCollector).GetAnimationProviders();

            foreach (var child in children)
            {
                child.WriteAnimation(source, context);
            }

        }
    }
}