using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor.AssetImporters;
using UnityEditor.Graphs;

namespace Numeira.HarmonyPatch;

internal sealed class AddComponentMenuPatch : HarmonyPatch<AddComponentMenuPatch>
{
    protected override void Patch(Harmony harmony)
    {
        var propertyEditorType = typeof(Editor).Assembly.GetTypes().FirstOrDefault(x => x.FullName is "UnityEditor.PropertyEditor");
        if (propertyEditorType is null)
            return;

        var method = propertyEditorType.GetMethod("AddComponentButton", BindingFlags.NonPublic | BindingFlags.Instance);
        if (method is null)
            return;

        harmony.Patch(method, prefix: GetPatchMethod(nameof(PatchAddComponentButton)));
    }

    public static Action<object, float> DrawSplitLine
    {
        get
        {
            if (drawSplitLine is null)
            {
                var propertyEditorType = typeof(Editor).Assembly.GetTypes().FirstOrDefault(x => x.FullName is "UnityEditor.PropertyEditor");

                var method = new DynamicMethod(nameof(DrawSplitLine), null, new[] { typeof(object), typeof(float) }, propertyEditorType, true);

                var original = propertyEditorType.GetMethod("DrawSplitLine", BindingFlags.NonPublic | BindingFlags.Instance);
                var il = method.GetILGenerator();

                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(OpCodes.Call, original);
                il.Emit(OpCodes.Ret);

                drawSplitLine = method.CreateDelegate(typeof(Action<object, float>)) as Action<object, float>;
            }
            return drawSplitLine!;
        }
    }
    private static Action<object, float>? drawSplitLine;

    private static void PatchAddComponentButton(object __instance, Editor[] editors)
    {
        var assetImporter = GetAssetImporter(editors);
        if (assetImporter != null && !assetImporter.showImportedObject)
            return;

        var editor = GetFirstNonImportInspectorEditor(editors);

        if (editor == null || editor.target == null || editor.target is not GameObject go)
            return;

        if (go.TryGetComponent<IInspectorExtension>(out var ie))
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            if (Event.current.type is EventType.Repaint)
                DrawSplitLine?.Invoke(__instance, rect.y);

            EditorGUILayout.Space();


            EditorGUIUtility.labelWidth = 150f;
            try
            {
                var c = InspectorExtensionContext.Instance;
                c.ClearRegisteredComponentShortcut();
                ie.OnInspectorExtension(c);
                c.DoComponentShortcut(go);
            }
            finally
            {
                EditorGUIUtility.labelWidth = 0f;
            }
            EditorGUILayout.Space();
        }
    }

    private sealed class InspectorExtensionContext : IInspectorExtensionContext
    {
        public static InspectorExtensionContext Instance { get; } = new();

        private sealed class IndentLevelDisposable : IDisposable
        {
            public static IndentLevelDisposable Instance { get; } = new();
            public void Dispose()
            {
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
        }

        private sealed class HorizontalOrVerticalDisposable : IDisposable
        {
            private readonly bool isVertical;

            public static HorizontalOrVerticalDisposable Horizontal { get; } = new(false);
            public static HorizontalOrVerticalDisposable Vertical { get; } = new(true);

            public HorizontalOrVerticalDisposable(bool isVertical) => this.isVertical = isVertical;

            public void Dispose()
            {
                if (isVertical)
                    EditorGUILayout.EndVertical();
                else
                    EditorGUILayout.EndHorizontal();
            }
        }

        private sealed class DisabledDisposable : IDisposable
        {
            public static DisabledDisposable Instance { get; } = new();
            public void Dispose()
            {
                EditorGUI.EndDisabledGroup();
            }
        }

        public IDisposable BeginGroup(string label, ref bool open)
        {
            open = EditorGUILayout.BeginFoldoutHeaderGroup(open, label);
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUI.indentLevel++;

            return IndentLevelDisposable.Instance;
        }

        public IDisposable BeginHorizontal()
        {
            EditorGUILayout.BeginHorizontal();
            return HorizontalOrVerticalDisposable.Horizontal;
        }

        public IDisposable BeginVertical()
        {
            EditorGUILayout.BeginVertical();
            return HorizontalOrVerticalDisposable.Vertical;
        }

        public IDisposable BeginDisabled(bool disable)
        {
            EditorGUI.BeginDisabledGroup(disable);
            return DisabledDisposable.Instance;
        }

        public bool CheckBox(string label, bool on, Action<bool>? callback = null)
        {
            callback ??= static _ => { };
            return RuntimeGUIUtils.ChangeCheck(() => EditorGUILayout.Toggle(label, on), callback);
        }

        public bool Button(string label, Action? callback = null)
        {
            callback ??= () => { };
            var c = EditorGUIUtility.TrTempContent(label);

            var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 1.5f);
            rect = EditorGUI.IndentedRect(rect);

            if (GUI.Button(rect, c))
            {
                callback();
                return true;
            }
            return false;
        }

        private readonly List<(string Title, Action<GameObject> Callback)> registeredComponentShortcut = new();

        public void ClearRegisteredComponentShortcut() => registeredComponentShortcut.Clear();

        public void RegisterComponentShortcut(string title, Action<GameObject> callback)
        {
            registeredComponentShortcut.Add((title, callback));
        }

        public void DoComponentShortcut(GameObject gameObject)
        {
            bool flag = true;
            using (BeginGroup("Component Shortcuts", ref flag))
            {
                foreach(var (title, callback) in registeredComponentShortcut.AsSpan())
                {
                    if (Button(title))
                    {
                        callback(gameObject);
                    }
                }
            }
        }
    }

    private static Editor? GetFirstNonImportInspectorEditor(Editor[] editors)
    {
        foreach (Editor editor in editors)
        {
            if (editor.target is not AssetImporter)
            {
                return editor;
            }
        }

        return null;
    }

    private static AssetImporterEditor? GetAssetImporter(Editor[] editors)
    {
        if (editors == null || editors.Length == 0)
            return null;

        return editors[0] as AssetImporterEditor;
    }

    internal ref struct ColorScope
    {
        private Color color;

        public ColorScope(Color newColor)
        {
            color = GUI.color;
            GUI.color = newColor;
        }

        public void Dispose()
        {
            GUI.color = color;
        }
    }
}
