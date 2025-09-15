
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using nadena.dev.ndmf.runtime;
using nadena.dev.ndmf.util;
using UnityEditor.SceneManagement;
using VRC.SDK3.Avatars.Components;

namespace Numeira;

[InitializeOnLoad]
internal static class CreationContextMenu
{
    private sealed class StaticExtension { }

    static CreationContextMenu()
    {
        EditorApplication.delayCall += () =>
        {
            SceneHierarchyHooks.addItemsToGameObjectContextMenu += default(StaticExtension).AddItemsToGameObjectContextMenu;
        };
    }

    private static Type DefaultConditionType =>
#if VRC_SDK_VRCSDK3
        typeof(ModEmoGestureCondition);
#else
        typeof(ModEmoCondition);
#endif

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

        AddMenu("ModEmo/Create Pattern", () => CreateNewObject("Expression Pattern (1)", go, typeof(ModEmoExpressionPattern)), enabled: go.GetComponent<ModEmo>() != null);
        
        menu.AddSeparator("ModEmo/");
        AddMenu("ModEmo/Create Expression", () => CreateNewExpression("Expression (1)", go), enabled: go.GetComponent<IModEmoExpressionPattern>() != null);
        AddMenu("ModEmo/Create Empty Expression", () => CreateNewExpression("Expression (1)", go, empty: true), enabled: go.GetComponent<IModEmoExpressionPattern>() != null);
        AddMenu("ModEmo/Create Simplify Expression", () => CreateNewObject("Expression (1)", go, typeof(ModEmoDefaultExpression), DefaultConditionType, typeof(ModEmoBlendShapeSelector)), enabled: go.GetComponent<IModEmoExpressionPattern>() != null);

        menu.AddSeparator("ModEmo/");

        AddMenu("ModEmo/Import Pattern from Avatar FX Layer ..", () => ImportPatternFromFXLayer(go), go.GetComponent<ModEmo>() != null);

    }

    private static void ImportPatternFromFXLayer(GameObject parent)
    {
        var avatar = RuntimeUtil.FindAvatarInParents(parent.transform);
        string path;
        if (avatar is not null && avatar.GetComponent<VRCAvatarDescriptor>() is { } desc && desc.baseAnimationLayers.FirstOrDefault(x => x.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController is { } fx)
        {
            path = Path.GetDirectoryName(AssetDatabase.GetAssetPath(fx));
        }
        else
        {
            path = "Assets";
        }

        path = EditorUtility.OpenFilePanel("Open animator controller", path, "controller");
        if (string.IsNullOrEmpty(path))
            return;
        path = Path.GetRelativePath(Path.GetDirectoryName(Application.dataPath), path);
        Debug.LogError(path);
        var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        if (animatorController == null)
            return;

        var go = PatternImporter.ImportFromAnimatorController(animatorController);
        CreateNewObject(go, parent);
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

        var sampleCondition = ObjectFactory.CreateGameObject("Condition", DefaultConditionType);
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
