using nadena.dev.modular_avatar.core;
using VRC.Core;
using ExpressionsMenuControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;

namespace Numeira;

internal class MenuGenerator
{
    private BuildContext context;
    public MenuGenerator(BuildContext context)
    {
        this.context = context;
    }

    public void Generate()
    {
        var rootObj = context.GetModEmoContext().Root.gameObject;
        var menuRoot = rootObj.AddComponent<ModularAvatarMenuItem>();
        menuRoot.MenuSource = SubmenuSource.Children;
        menuRoot.Control.type = ExpressionsMenuControlType.SubMenu;

        GenerateBlendShapeControlMenu(menuRoot);
    }

    private void GenerateBlendShapeControlMenu(ModularAvatarMenuItem menuRoot)
    {
        var generated = context.GetData().GeneratedBlendshapeControls;
        if (generated is null)
            return;

        var paramters = menuRoot.gameObject.GetOrAddComponent<ModularAvatarParameters>();

        var menuRootObj = new GameObject("BlendShapes");
        menuRootObj.transform.parent = menuRoot.transform;
        menuRoot = menuRootObj.AddComponent<ModularAvatarMenuItem>();
        menuRoot.MenuSource = SubmenuSource.Children;
        menuRoot.Control.type = ExpressionsMenuControlType.SubMenu;


        foreach (var (group, shapes) in context.GetData().CategorizedBlendShapes)
        {
            ModularAvatarMenuItem? menuGroup = null;
            foreach (var shape in shapes)
            {
                if (!generated.Contains(shape.Key))
                    continue;

                if (menuGroup is null)
                {
                    var menuObj = new GameObject(group);
                    menuObj.transform.parent = menuRoot.transform;
                    menuGroup = menuObj.AddComponent<ModularAvatarMenuItem>();
                    menuGroup.MenuSource = SubmenuSource.Children;
                    menuGroup.Control.type = ExpressionsMenuControlType.SubMenu;
                }

                var obj = new GameObject(shape.Key);
                obj.transform.parent = menuGroup.transform;
                var menu = obj.AddComponent<ModularAvatarMenuItem>();
                menu.Control.type = ExpressionsMenuControlType.RadialPuppet;
                var name = $"{ParameterNames.Internal.BlendShapes.OverridePrefix}{shape.Key}";
                paramters.parameters.Add(new() { nameOrPrefix = name, syncType = ParameterSyncType.Float, localOnly = true, });
                menu.Control.subParameters = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter[] { new() { name = name} };
            }
        }
    }
}