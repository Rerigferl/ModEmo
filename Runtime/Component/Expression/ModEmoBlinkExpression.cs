namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Expression")]
    internal sealed class ModEmoBlinkExpression : ModEmoExpression, IModEmoLoopControl, IAnimationSourceComponent
    {
        public bool IsLoop => true;
        public override int LayerIndex => 1;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            base.CalculateContentHash(ref hashCode);
        }

        public void RegisterAnimations(IAnimationRegistry registry, in AnimationGeneratorOptions options)
        {
            var context = registry.RegisterAnimation(options);
            var writer = new WriterContext(context);

            foreach (var x in (this as IOwnerComponent<IKeyframeWriterComponent>).GetOwnedComponents())
            {
                x.WriteKeyframes(writer, options);
            }
        }

        private sealed class WriterContext : IKeyframeWriterContext
        {
            public IKeyframeWriterContext Source { get; }

            public WriterContext(IKeyframeWriterContext source)
            {
                Source = source;
            }

            public void AddBlendshape(Transform target, string name, float time, float value)
            {
                Source.AddBlendshape(target, name, 0 / 60f, 0);
                Source.AddBlendshape(target, name, 60 / 60f, 0);
                Source.AddBlendshape(target, name, 65 / 60f, value);
                Source.AddBlendshape(target, name, 67 / 60f, value);
                Source.AddBlendshape(target, name, 80 / 60f, 0);
                Source.AddBlendshape(target, name, 300 / 60f, 0);
            }

            public void AddCancelBlendshape(Transform target, string name, float time, float value)
            {
                Source.AddCancelBlendshape(target, name, 0 / 60f, 0);
                Source.AddCancelBlendshape(target, name, 60 / 60f, 0);
                Source.AddCancelBlendshape(target, name, 65 / 60f, value);
                Source.AddCancelBlendshape(target, name, 67 / 60f, value);
                Source.AddCancelBlendshape(target, name, 80 / 60f, 0);
                Source.AddCancelBlendshape(target, name, 300 / 60f, 0);
            }

            public void AddRotation(Transform target, float time, Vector3 eularAngle, bool relative = true)
            {
                Source.AddRotation(target, time, eularAngle, relative);
            }

            public void SetAvatarParameter<T>(string name, T value)
            {
                Source.SetAvatarParameter(name, value);
            }

            public void SetAnimatorParameter<T>(string name, float time, T value)
            {
                Source.SetAnimatorParameter(name, time, value);
            }
        }
    }
}