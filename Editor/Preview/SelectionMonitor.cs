using nadena.dev.ndmf.preview;

namespace Numeira;

[InitializeOnLoad]
internal static class SelectionMonitor
{
    public static PublishedValue<GameObject[]> Selection { get; } = new(Array.Empty<GameObject>(), "numeira.mod-emo.selection-monitor.selection");
    public static PublishedValue<GameObject?> Active { get; } = new(null, "numeira.mod-emo.selection-monitor.active");

    static SelectionMonitor()
    {
        EditorApplication.delayCall += () =>
        {
            UnityEditor.Selection.selectionChanged += default(object).OnSelectionChanged;
        };
    }

    private static void OnSelectionChanged(this object? __)
    {
        Selection.Value = UnityEditor.Selection.gameObjects ?? Array.Empty<GameObject>();
        Active.Value = UnityEditor.Selection.activeGameObject;
    }
}