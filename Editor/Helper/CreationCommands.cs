using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using nadena.dev.ndmf.runtime;
using nadena.dev.ndmf.runtime.components;
using nadena.dev.ndmf.util;
using UnityEditor.SceneManagement;
using VRC.SDK3.Avatars.Components;
#if FACE_EMO
using Suzuryg.FaceEmo.Components;
#endif

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

        ModEmoExpressionEditorBase.CreateNewObject = CreateNewObject;
    }

    private static Type DefaultConditionType =>
#if VRC_SDK_VRCSDK3
        typeof(ModEmoGestureCondition);
#else
        typeof(ModEmoCondition);
#endif

    private static bool IsAvatarRoot(GameObject go)
    {
        if (go == null) 
            return false;

#if VRC_SDK_VRCSDK3
        if (go.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() != null)
            return true;
#endif

        return go.GetComponent<NDMFAvatarRoot>() != null;
    }

    private static void AddItemsToGameObjectContextMenu(this StaticExtension? __, GenericMenu menu, GameObject go)
    {
        if (IsAvatarRoot(go))
        {
            menu.AddSeparator("");
            AddMenu("ModEmo/Create New", () =>
            {
                if (go.GetComponentInChildren<ModEmo>() is { } c)
                {
                    Selection.activeGameObject = c.gameObject;
                    EditorGUIUtility.PingObject(c);
                    return;
                }

                CreateNewObject("ModEmo", go, typeof(ModEmo));
            });
            return;
        }

        if (go.GetComponentInParent<ModEmo>() == null)
            return;

        menu.AddSeparator("");

        bool isRoot = go.GetComponent<ModEmo>() != null;
        bool isPattern = go.GetComponent<IModEmoExpressionPattern>() != null;
        bool isExpressionFolder = go.GetComponent<IModEmoExpressionFolder>() != null;
        bool isExpression = go.GetComponentInParent<IModEmoExpression>() != null;
        bool isFrameFolder = go.GetComponent<ModEmoExpressionFrameFolder>() != null;

        AddMenu("ModEmo/Create Pattern", () => CreateNewObject("Expression Pattern (1)", go, typeof(ModEmoExpressionPattern)), enabled: isRoot);
        
        menu.AddSeparator("ModEmo/");
        AddMenu("ModEmo/Expression/Create Expression", () => CreateNewExpression("Expression (1)", go), enabled: isExpressionFolder);
        AddMenu("ModEmo/Expression/Create Empty Expression", () => CreateNewExpression("Expression (1)", go, empty: true), enabled: isExpressionFolder);
        AddMenu("ModEmo/Expression/Create Simplify Expression", () => CreateNewObject("Expression (1)", go, typeof(ModEmoDefaultExpression), DefaultConditionType, typeof(ModEmoBlendShapeSelector)), enabled: isExpressionFolder);

        menu.AddSeparator("ModEmo/Expression/");

        AddMenu("ModEmo/Expression/Frame/Add Frame", () => CreateNewObject("Frame (1)", go, typeof(ModEmoExpressionFrame)), enabled: isExpression || isFrameFolder);

        menu.AddSeparator("ModEmo/");

        AddMenu("ModEmo/Folder/Create Expression Folder", () => CreateNewObject("Expression Folder", go, typeof(ModEmoExpressionFolder)), enabled: isExpressionFolder);
        AddMenu("ModEmo/Folder/Create Expression Frame Folder", () => CreateNewObject("Expression Frame Folder", go, typeof(ModEmoExpressionFolder)), enabled: isExpression);

        menu.AddSeparator("ModEmo/");

        AddMenu("ModEmo/Import Pattern from Avatar FX Layer ..", () => ImportPatternFromFXLayer(go), isRoot);

#if FACE_EMO

        var faceEmo = Selection.gameObjects?.FirstOrDefault(x => x.TryGetComponent<FaceEmoLauncherComponent>(out _));

        AddMenu("ModEmo/Import Pattern from FaceEmo ..", () => 
        {
            if (faceEmo == null)
                return;

            var patterns = PatternImporter.ImportFromFaceEmo(faceEmo);
            if (patterns is null)
                return;

            foreach(var pattern in patterns)
            {
                pattern.transform.parent = go.transform;
            }

        }, faceEmo != null);
#endif

        void AddMenu(string title, GenericMenu.MenuFunction callback, bool enabled = true)
        {
            var content = new GUIContent(title);
            if (enabled)
                menu.AddItem(content, false, callback);
            else
                menu.AddDisabledItem(content, false);
        }
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
