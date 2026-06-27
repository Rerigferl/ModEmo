#if UNITY_EDITOR

namespace Numeira;

internal static class RuntimeGUIUtils
{
    private static GUIContent? temporaryGUIContent;

    public static GUIContent ToTempGUIContent(this string text)
    {
        var g = temporaryGUIContent ??= new();
        g.text = text;
        return g;
    }

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