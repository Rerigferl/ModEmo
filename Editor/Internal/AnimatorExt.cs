using Numeira.Animation;

namespace Numeira;

internal static class AnimatorExt
{
    public static void Deconstruct(this ChildAnimatorState value, out AnimatorState state, out Vector3 position)
    {
        state = value.state;
        position = value.position;
    }

    public static void Commit(in this AnimatorParameterCondition condition, List<TransitionBuilder> transitions)
    {
        switch (condition.Parameter.Value.Type)
        {
            case AnimatorControllerParameterType.Bool:
                {
                    switch (condition.Mode)
                    {
                        case ConditionMode.Equals:
                        case ConditionMode.NotEqual:
                            {
                                foreach (var transition in transitions.AsSpan())
                                {
                                    transition.AddCondition(condition.Parameter.Value == (condition.Mode is ConditionMode.Equals) ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, condition.Parameter.Name, 0);
                                }
                            }
                            break;
                    }
                }
                break;

            case AnimatorControllerParameterType.Int:
                {
                    switch (condition.Mode)
                    {
                        case ConditionMode.NotEqual:
                        case ConditionMode.Equals:
                            {
                                foreach (var transition in transitions.AsSpan())
                                {
                                    transition.AddCondition(condition.Mode is ConditionMode.Equals ? AnimatorConditionMode.Equals : AnimatorConditionMode.NotEqual, condition.Parameter.Name, condition.Parameter.Value.Value);
                                }
                            }
                            break;
                    }
                }
                break;

                // TODO...
        }
    }
}