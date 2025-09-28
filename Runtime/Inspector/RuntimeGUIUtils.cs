#if UNITY_EDITOR
namespace Numeira;
internal static class RuntimeGUIUtils
{
    public static T ChangeCheck<T>(Func<T> gui, Action<T> callback)
    {
        EditorGUI.BeginChangeCheck();
        var value = gui();
        if (EditorGUI.EndChangeCheck() )
        {
            callback(value);
        }
        return value;
    }
}
#endif