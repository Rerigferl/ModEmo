using System.Collections.Immutable;
using nadena.dev.modular_avatar.core;
using VRC.Core;
using ExpressionsMenuControlType = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType;
using MenuItem = nadena.dev.modular_avatar.core.ModularAvatarMenuItem;

namespace Numeira;

internal class MenuGenerator
{
    private BuildContext Context { get; }
    private ModEmo Component { get; }
    
    public MenuGenerator(BuildContext context)
    {
        Context = context;
        Component = context.GetModEmoContext().Root;
    }

    public void Generate()
    {
        var menuRoot = AddMenu("ModEmo", PortableControlType.SubMenu);
        menuRoot.gameObject.AddComponent<ModularAvatarMenuInstaller>();

        var parameters = menuRoot.gameObject.AddComponent<ModularAvatarParameters>();

        parameters.GetOrAdd(ParameterNames.Expression.Pattern).syncType = ParameterSyncType.Int;

        var expressionLock = AddMenu("Lock", PortableControlType.Toggle, menuRoot);
        expressionLock.PortableControl.Parameter = ParameterNames.Expression.Lock;

        var patterns = Context.GetData().Expressions.GroupBy(x => x.PatternIndex).ToArray();
        if (patterns.Length > 1)
        {
            var patternsFolder = AddMenu("Patterns", PortableControlType.SubMenu, menuRoot);
            
            foreach (var pattern in patterns)
            {
                var a = pattern.First();
                var menu = AddMenu(a.Pattern.Name, PortableControlType.Toggle, patternsFolder);
                menu.PortableControl.Parameter = ParameterNames.Expression.Pattern;
                menu.PortableControl.Value = a.PatternIndex;
            }
        }

        var blinkLock = AddMenu("Blink", PortableControlType.Toggle, menuRoot);
        blinkLock.PortableControl.Parameter = ParameterNames.Blink.Sync;
        blinkLock.isDefault = true;

        var blendShapeMenu = AddMenu("BlendShapes", PortableControlType.SubMenu, menuRoot);
        var data = Context.GetData();
        string[] singleArray = new string[1];
        foreach(var (key, values) in data.CategorizedBlendShapes)
        {
            MenuItem? menu = null;;
            foreach (var value in values)
            {
                if (!data.UsageBlendShapeMap.Contains(value))
                    continue;
                menu ??= AddMenu(key, PortableControlType.SubMenu, blendShapeMenu);
                var a = AddMenu(value, PortableControlType.RadialPuppet, menu);
                var name = singleArray[0] = $"{ParameterNames.Internal.BlendShapes.Prefix}{value}/Override";
                parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = name, syncType = ParameterSyncType.Float, localOnly = true, saved = false });
                a.PortableControl.SubParameters = singleArray.ToImmutableList();
            }
        }
    }

    private MenuItem AddMenu(string name, PortableControlType type, MenuItem? parent = null)
    {
        var go = new GameObject(name);
        go.transform.parent = parent?.transform ?? Context.AvatarRootTransform;

        var menuItem = go.AddComponent<MenuItem>();
        menuItem.MenuSource = SubmenuSource.Children;
        menuItem.PortableControl.Type = type;
        return menuItem;
    }
}

internal static partial class Ext
{
    public static ref ParameterConfig GetOrAdd(this ModularAvatarParameters parameters, string name)
    {
        var list = parameters.parameters;
        foreach(ref var parameter in list.AsSpan())
        {
            if (parameter.nameOrPrefix == name)
                return ref parameter;
        }

        list.Add(new ParameterConfig() { nameOrPrefix = name });
        return ref list.AsSpan()[^1];
    }
}