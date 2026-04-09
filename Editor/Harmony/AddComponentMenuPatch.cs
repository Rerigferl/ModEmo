using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEditor.AssetImporters;
using UnityEditor.Graphs;
using static UnityEditor.PlayerSettings;

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

        if (go.GetComponent<IModEmoExpression>() is { } expression)
        {
            var rect = EditorGUILayout.GetControlRect(false, 1);
            if (Event.current.type is EventType.Repaint)
                DrawSplitLine?.Invoke(__instance, rect.y);

            EditorGUILayout.Space();

            ModEmoExpressionEditor.OnFooterGUI(editor, expression);

            EditorGUILayout.Space();
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
