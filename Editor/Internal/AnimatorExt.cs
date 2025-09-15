namespace Numeira;

internal static class AnimatorExt
{
    public static void Deconstruct(this ChildAnimatorState value, out AnimatorState state, out Vector3 position)
    {
        state = value.state;
        position = value.position;
    }
}