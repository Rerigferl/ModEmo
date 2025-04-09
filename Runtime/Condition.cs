namespace Numeira;

[Serializable]
internal struct Condition
{
    public Hand Hand;
    public Gesture Gesture;
    [Range(0, 1)]
    public float Weight;
}

[Serializable]
internal sealed class VRChatExpressionCondition : IModEmoExpressionCondition
{
    public Hand Hand;
    public Gesture Gesture;

#if UNITY_EDITOR
    public IEnumerable<AnimatorCondition> ToAnimatorConditions()
    {
        if (Hand.HasFlag(Hand.Left))
            yield return CreateGestureCondition("GestureLeft");

        if (Hand.HasFlag(Hand.Right))
            yield return CreateGestureCondition("GestureRight");
    }

    private AnimatorCondition CreateGestureCondition(string parameterName)
        => new() { parameter = parameterName, mode = AnimatorConditionMode.Equals, threshold = (int)Gesture };
#endif
}

internal interface IModEmoExpressionCondition
{
#if UNITY_EDITOR
    IEnumerable<AnimatorCondition> ToAnimatorConditions();
#endif
}