namespace Numeira;

internal static class AnimationUtils
{
#if UNITY_EDITOR
    public static EditorCurveBinding CreateAAPBinding(string name)
        => new() { path = "", propertyName = name, type = typeof(Animator) };
#endif
}
