namespace Numeira
{
    internal interface IModEmoExpression : IModEmoComponent
    {
        string Name { get; }

        ExpressionMode Mode { get; }

        IEnumerable<ExpressionFrame> Frames { get; }

        IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> Conditions => Component.GetComponentsInDirectChildren<IModEmoConditionProvider>(includeSelf: true).SelectMany(x => x);

        bool IsLoop => Component.GetComponent<IModEmoLoopControl>()?.IsLoop is true;

        string? MotionTime => Component.GetComponent<IModEmoMotionTimeProvider>()?.ParameterName;

        bool? Blink => Component.GetComponent<IModEmoBlinkControl>()?.Enable;

        bool LipSync => Component.GetComponent<IModEmoLipSyncConttrol>()?.Enable ?? true;

        bool EnableMouthMorphCancel => Component.GetComponent<IModEmoMouthMorphCancelControl>()?.Enable ?? true;
    }
}