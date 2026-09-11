
using UnityEngine.UIElements;

namespace Numeira
{
    internal sealed class ModEmoCompositeExpression : ModEmoNamedTagComponent, IModEmoExpression, IOwnerComponent<IModEmoExpression>
    {
        public ExpressionMode Mode => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {

        }

        public void RegisterAnimations(IAnimationRegistry registry, in AnimationGeneratorOptions options)
        {
            var expressions = this.GetOwnedComponents<IModEmoExpression>().ToArray();
            var motionTimes = this.GetOwnedComponents<IModEmoMotionTimeProvider>().ToArray();
            if (expressions.Length < 2 || motionTimes.Length < 2)
                return;

            var factory = registry.RegisterTwoAxisAnimation(motionTimes[0].ParameterName ?? "", motionTimes[1].ParameterName ?? "");

            {
                var registry2 = factory.GetRegistry(Vector2.zero);
                var context = registry2.RegisterAnimation(options);

                foreach(var x in this.GetOwnedComponents<IKeyframeWriterComponent>())
                {
                    x.WriteKeyframes(context, options);
                }
            }

            for (int i = 0; i < expressions.Length; i++)
            {
                IModEmoExpression? expression = expressions[i];
                int idx = i + 1;
                var position = expression.GetOwnedComponents<IPositionProviderComponent>().FirstOrDefault()?.Position ?? new(idx % 2, idx / 2);

                expression.RegisterAnimations(factory.GetRegistry(position), options);
            }
        }
    }
}