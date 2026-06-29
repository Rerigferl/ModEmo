
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Expression")]
    internal sealed class ModEmoBlinkExpression : ModEmoExpression, IModEmoLoopControl, IModEmoAnimationCollector
    {
        public bool IsLoop => true;
        public override int LayerIndex => 1;

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
            var collector = AnimationWriter.Shared;
            
            collector.Curves.Clear();
            foreach(var child in children)
            {
                child.WriteAnimation(collector, context);
            }

            foreach (var (binding, curve) in collector.Curves)
            {
                var value = curve.Evaluate(0);
                source.Write(binding, 0 / 60f, 0);
                source.Write(binding, 60 / 60f, 0);
                source.Write(binding, 65 / 60f, value);
                source.Write(binding, 67 / 60f, value);
                source.Write(binding, 80 / 60f, 0);
                source.Write(binding, 300 / 60f, 0);
            }
        }
    }
}