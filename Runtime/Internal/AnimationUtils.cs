using System.Reflection;
using System.Reflection.Emit;

namespace Numeira;

[InitializeOnLoad]
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

    public static void SetEditorCurveNoSync(this AnimationClip clip, EditorCurveBinding binding, AnimationCurve curve)
    {
        _Internal_SetEditorCurve?.Invoke(clip, binding, curve, false);
    }

    public static void SyncEditorCurves(this AnimationClip clip)
    {
        _SyncEditorCurves?.Invoke(clip);
    }

    private static readonly Internal_SetEditorCurve? _Internal_SetEditorCurve;
    private static readonly SyncEditorCurvesDelegate? _SyncEditorCurves;

    private delegate void Internal_SetEditorCurve(AnimationClip clip, EditorCurveBinding binding, AnimationCurve curve, bool syncEditorCurves);
    private delegate void SyncEditorCurvesDelegate(AnimationClip clip);

    static AnimationUtils()
    {
        var method = new DynamicMethod(nameof(Internal_SetEditorCurve), null, new[] { typeof(AnimationClip), typeof(EditorCurveBinding), typeof(AnimationCurve), typeof(bool) }, typeof(AnimationUtility), true);
        var original = typeof(AnimationUtility).GetMethod(nameof(Internal_SetEditorCurve), BindingFlags.Static | BindingFlags.NonPublic);
        var il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Ldarg_3);
        il.Emit(OpCodes.Call, original);
        il.Emit(OpCodes.Ret);
        _Internal_SetEditorCurve = method.CreateDelegate(typeof(Internal_SetEditorCurve)) as Internal_SetEditorCurve;

        method = new DynamicMethod(nameof(SyncEditorCurves), null, new[] { typeof(AnimationClip) }, typeof(AnimationUtility), true);
        original = typeof(AnimationUtility).GetMethod(nameof(SyncEditorCurves), BindingFlags.Static | BindingFlags.NonPublic);
        il = method.GetILGenerator();
        il.Emit(OpCodes.Ldarg_0);
        il.Emit(OpCodes.Call, original);
        il.Emit(OpCodes.Ret);
        _SyncEditorCurves = method.CreateDelegate(typeof(SyncEditorCurvesDelegate)) as SyncEditorCurvesDelegate;

    }

#endif
}
