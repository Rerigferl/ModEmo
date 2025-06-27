namespace Numeira;

internal static class AnimationUtils
{
#if UNITY_EDITOR
    public static EditorCurveBinding CreateAAPBinding(string name)
        => new() { path = "", propertyName = name, type = typeof(Animator) };

    public static AnimationClip CreateAAPClip(string name, float value)
    {
        var motion = new AnimationClip() { name = $"{name} {value}" };
        var bind = CreateAAPBinding(name);
        AnimationUtility.SetEditorCurve(motion, bind, AnimationCurve.Constant(0, 0, value));
        return motion;
    }
#endif
}
