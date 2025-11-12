namespace Numeira
{
    internal interface IModEmoExpression : IModEmoComponent, IModEmoAnimationCollector
    {
        string Name { get; }

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

        string? MotionTime => Component.GetComponent<IModEmoMotionTimeProvider>()?.ParameterName;

        bool Blink => Component.GetComponent<IModEmoBlinkControl>()?.Enable ?? true;

        bool LipSync => Component.GetComponent<IModEmoLipSyncConttrol>()?.Enable ?? true;

        bool EnableMouthMorphCancel => Component.GetComponent<IModEmoMouthMorphCancelControl>()?.Enable ?? false;
    }
}