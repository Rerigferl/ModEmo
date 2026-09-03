
namespace Numeira
{
    internal abstract class ModEmoExpression : ModEmoNamedTagComponent, IModEmoExpression
    {
        ExpressionMode IModEmoExpression.Mode => GetMode();

        public virtual int LayerIndex => 0;

        public IEnumerable<string> MotionTime
        {
            get
            {
                var motionTimes = GetComponents<IModEmoMotionTimeProvider>();
                var subExpressions = this.GetComponentsInDirectChildren<IModEmoExpression>();
                if (subExpressions.Length >= 2 && motionTimes.Length >= 2)
                {
                    yield return motionTimes[0].ParameterName!;
                    yield return motionTimes[1].ParameterName!;
                    yield break;
                }

                if (motionTimes.Length < 1 || string.IsNullOrEmpty(motionTimes[0].ParameterName))
                    yield break;
                yield return motionTimes[0].ParameterName!;
            }
        }

        protected virtual ExpressionMode GetMode() => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(GetName().GetFarmHash64());
            foreach (var frame in this.GetComponentsInDirectChildren<IModEmoBlendshapeConsumer>(includeSelf: true))
            {
                frame.CalculateContentHash(ref hashCode);
            }
            foreach (var condition in this.GetComponentsInDirectChildren<IModEmoConditionProvider>(includeSelf: true))
            {
                condition.CalculateContentHash(ref hashCode);
            }
        }
    }
}