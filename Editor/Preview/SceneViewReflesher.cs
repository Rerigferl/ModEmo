namespace Numeira;

internal static class SceneViewReflesher
{
    private static Stack<bool> refleshStack = new();

    static SceneViewReflesher()
    {
        EditorApplication.update += () =>
        {
            if (refleshStack.Count == 0)
                return;

            if (!EditorApplication.isFocused)
                return;

            SceneView.RepaintAll();
        };
    }

    public static IDisposable BeginReflesh()
    {
        refleshStack.Push(true);
        return RefleshDisposer.Instance;
    }

    private sealed class RefleshDisposer : IDisposable
    {
        public static readonly RefleshDisposer Instance = new();
        public void Dispose()
        {
            refleshStack.Pop();
        }
    }
}
