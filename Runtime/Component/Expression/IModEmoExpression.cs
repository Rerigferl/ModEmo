namespace Numeira
{
    internal interface IModEmoExpression : IModEmoNamedComponent, IAnimationSourceComponent, IOwnerComponent<IModEmoMotionTimeProvider>, ISubComponent<IModEmoExpression>, IOwnerComponent<IPositionProviderComponent>
    {
        ExpressionMode Mode { get; }

        IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> Conditions => GetConditions();

        private IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            var self = this.GameObject.GetComponents<IModEmoConditionProvider>();
            if (self.Length != 0)
            {
                yield return Group.Create(self[0], self.SelectMany(x => x.GetConditions()).SelectMany(x => x));
            }

            foreach (var x in Component.GetComponentsInDirectChildren<IModEmoConditionProvider>().SelectMany(x => x.GetConditions()))
                yield return x;
        }

        bool IsLoop => Component.GetComponent<IModEmoLoopControl>()?.IsLoop is true;

        bool Blink => Component.GetComponent<IModEmoBlinkControl>()?.Enable ?? true;

        bool LipSync => Component.GetComponent<IModEmoLipSyncControl>()?.Enable ?? true;

        bool EyeTracking => Component.GetComponent<IModEmoEyeTrackingControl>()?.Enable ?? true;

        bool EnableMouthMorphCancel => Component.GetComponent<IModEmoMouthMorphCancelControl>()?.Enable ?? false;

        void IAnimationSourceComponent.RegisterAnimations(IAnimationRegistry registry, in AnimationGeneratorOptions options)
        {
            var motionTimes = (this as IOwnerComponent<IModEmoMotionTimeProvider>).GetOwnedComponents().ToArray();
            var keyframeWriters = (this as IOwnerComponent<IKeyframeWriterComponent>).GetOwnedComponents();

            if (motionTimes.Length <= 1)
            {
                var motionTime = motionTimes.Length == 0 ? null : motionTimes[0].ParameterName;
                var anim = registry.RegisterAnimation(options, motionTime);

                foreach(var writer in keyframeWriters)
                {
                    writer.WriteKeyframes(anim, options);
                }
            }
            else
            {
                // TODO!
            }
        }
    }
}
