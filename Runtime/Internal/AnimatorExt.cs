namespace Numeira;

internal static class AnimatorExt
{
#if UNITY_EDITOR

    public static AnimatorCondition Not(this AnimatorCondition condition)
    {
        condition.mode = condition.mode switch
        {
            AnimatorConditionMode.If => AnimatorConditionMode.IfNot,
            AnimatorConditionMode.IfNot => AnimatorConditionMode.If,
            AnimatorConditionMode.Equals => AnimatorConditionMode.NotEqual,
            AnimatorConditionMode.NotEqual => AnimatorConditionMode.Equals,
            AnimatorConditionMode.Greater => AnimatorConditionMode.Less,
            AnimatorConditionMode.Less => AnimatorConditionMode.Greater,
            _ => condition.mode,
        };
        return condition;
    }

#endif
}