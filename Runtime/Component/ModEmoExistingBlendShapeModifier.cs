namespace Numeira
{
    [AddComponentMenu("ModEmo/Utilities/ModEmo Existing Blendshape Modifier")]
    [RequireComponent(typeof(ModEmoBlendShapeSelector))]
    internal sealed class ModEmoExistingBlendShapeModifier : ModEmoTagComponent, IPreviewable, IOwnerComponent<IKeyframeWriterComponent>
    {
        public string TargetBlendShapeName = "";
        private const string SelfProxyName = "${}";

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
        }

        public IEnumerable<BlendShape> GetBlendShapes()
        {
            foreach (var selector in this.GetComponentsInDirectChildren<ModEmoBlendShapeSelector>(includeSelf: true))
            {
                if (!selector.enabled)
                    continue;
                foreach (var x in selector.BlendShapes)
                {
                    if (x.Name is SelfProxyName)
                        yield return x with { Name = TargetBlendShapeName };
                    else
                        yield return x;
                }
            }
        }

        public void RegisterAnimations(IAnimationRegistry registry, in AnimationGeneratorOptions options)
        {
            var context = registry.RegisterAnimation(options);
            foreach(var writer in this.GetOwnedComponents())
            {
                writer.WriteKeyframes(context, options);
            }
        }
    }
}