#if UNITY_EDITOR
using System.Diagnostics.CodeAnalysis;

namespace Numeira;

internal static class RuntimeGUIUtils
{
    public static T ChangeCheck<T>(Func<T> gui, Action<T> callback)
    {
        EditorGUI.BeginChangeCheck();
        var value = gui();
        if (EditorGUI.EndChangeCheck())
        {
            callback(value);
        }
        return value;
    }

    public static T NullableField<T>(T? value, Func<T, T> gui) where T : struct
    {
        if (value is not { } v)
        {
            EditorGUI.showMixedValue = true;
            v = default;
        }
        v = gui(v);
        EditorGUI.showMixedValue = false;
        return v;
    }
}
#endif