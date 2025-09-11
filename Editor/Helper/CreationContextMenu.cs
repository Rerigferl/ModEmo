
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor.SceneManagement;

namespace Numeira;

[InitializeOnLoad]
internal static class CreationContextMenu
{
    public const string PathPrefix = "GameObject/";
    public const string CreateNewPatternPath = PathPrefix + "Create New Pattern";

    private sealed class StaticExtension { }

    static CreationContextMenu()
    {
        EditorApplication.delayCall += () =>
        {
            SceneHierarchyHooks.addItemsToGameObjectContextMenu += default(StaticExtension).AddItemsToGameObjectContextMenu;
        };
    }

    private static void AddItemsToGameObjectContextMenu(this StaticExtension? __, GenericMenu menu, GameObject go)
    {
        if (go.GetComponentInParent<ModEmo>() == null)
            return;

        menu.AddSeparator("");
        void AddMenu(string title, GenericMenu.MenuFunction callback, bool enabled = true)
        {
            var content = new GUIContent(title);
            if (enabled)
                menu.AddItem(content, false, callback);
            else
                menu.AddDisabledItem(content, false);
        }

        AddMenu("ModEmo/Create Pattern", () => CreateNewObject("Expression Pattern", go, typeof(ModEmoExpressionPattern)), enabled: go.GetComponent<ModEmo>() != null);
        menu.AddSeparator("ModEmo/");
        AddMenu("ModEmo/Create Expression", () => CreateNewExpression("Expression", go), enabled: go.GetComponent<IModEmoExpressionPattern>() != null);
        AddMenu("ModEmo/Create Empty Expression", () => CreateNewExpression("Expression", go, empty: true), enabled: go.GetComponent<IModEmoExpressionPattern>() != null);
    }

    private static void CreateNewExpression(string name, GameObject parent, bool empty = false)
    {
        var obj = ObjectFactory.CreateGameObject(name, typeof(ModEmoDefaultExpression));
        if (empty)
        {
            CreateNewObject(obj, parent);
            return;
        }

        var conditions = ObjectFactory.CreateGameObject("Conditions", typeof(ModEmoConditionFolder));
        var motions = ObjectFactory.CreateGameObject("Frames", typeof(ModEmoExpressionFrameFolder));

        var sampleCondition = ObjectFactory.CreateGameObject("Condition", typeof(ModEmoCondition));
        var sampleFrame = ObjectFactory.CreateGameObject("BlendShape", typeof(ModEmoBlendShapeSelector));

        ObjectFactory.PlaceGameObject(conditions, obj);
        ObjectFactory.PlaceGameObject(motions, obj);

        ObjectFactory.PlaceGameObject(sampleCondition, conditions);
        ObjectFactory.PlaceGameObject(sampleFrame, motions);

        CreateNewObject(obj, parent);
    }

    private static void CreateNewObject(string name, GameObject parent, params Type[] components)
    {
        var newObj = ObjectFactory.CreateGameObject(name, components);
        CreateNewObject(newObj, parent);
    }

    private static void CreateNewObject(GameObject go, GameObject parent)
    {
        ObjectFactory.PlaceGameObject(go, parent);
        Selection.activeObject = go;
        SceneHierarchyWindow.FrameAndRenameNewGameObject();
    }

    internal static class SceneHierarchyWindow
    {
        public static void FrameAndRenameNewGameObject() => _FrameAndRenameNewGameObject();

        private static readonly Action _FrameAndRenameNewGameObject;

        static SceneHierarchyWindow()
        {
            var method = new DynamicMethod("", null, Type.EmptyTypes);
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Call, typeof(EditorWindow).Module.GetType("UnityEditor.SceneHierarchyWindow").GetMethod(nameof(FrameAndRenameNewGameObject), BindingFlags.Static | BindingFlags.NonPublic));
            il.Emit(OpCodes.Ret);
            _FrameAndRenameNewGameObject = (Action)method.CreateDelegate(typeof(Action));
        }
    }
}
