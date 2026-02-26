using System.Collections.Immutable;
using System.IO;
using nadena.dev.modular_avatar.core;
using Numeira.Animation;
using MenuItem = nadena.dev.modular_avatar.core.ModularAvatarMenuItem;

namespace Numeira;

internal static class MenuGenerator
{
    public static void Generate(BuildContext context, AnimatorControllerBuilder builder)
    {
        var modEmoContext = context.GetModEmoContext();
        var menuRoot = modEmoContext.Root.gameObject.AddComponent<MenuItem>();
        menuRoot.PortableControl.Type = PortableControlType.SubMenu;
        menuRoot.MenuSource = SubmenuSource.Children;

        var parameters = context.GetModEmoContext().Root.gameObject.AddComponent<ModularAvatarParameters>();
        parameters.GetOrAdd(ParameterNames.Expression.Pattern).syncType = ParameterSyncType.Int;
        parameters.GetOrAdd(ParameterNames.Expression.Index).syncType = ParameterSyncType.Int;
        parameters.GetOrAdd(ParameterNames.Internal.BlendShapes.Reset, x => x with { localOnly = true, saved = false, }).syncType = ParameterSyncType.Int;
        builder.Parameters.AddInt(ParameterNames.Internal.BlendShapes.Reset, 0);

        menuRoot.AddToggle("Lock", ParameterNames.Expression.Lock).WithSaved(false);
        menuRoot.AddToggle("Blink", ParameterNames.Blink.Sync).WithSaved(false).WithDefault(true);

        var data = context.GetData();
        var patterns = data.Expressions.GroupBy(x => x.PatternIndex).ToArray();
        if (patterns.Length > 1)
        {
            var patternsFolder = menuRoot.AddMenu("Patterns");

            foreach (var pattern in patterns)
            {
                var a = pattern.First();
                var menu = patternsFolder.AddToggle(a.Pattern.Name, ParameterNames.Expression.Pattern, a.PatternIndex);
            }
        }

        if (data.Expressions is { } expressions)
        {
            static void GetPath(Transform tr, Stack<string> stack)
            {
                if (tr == null)
                    return;

                if (tr.GetComponent<IModEmoExpression>() is { } ex)
                {
                    stack.Push(ex.Name);
                }
                else if (tr.GetComponent<IModEmoExpressionFolder>() is { } folder)
                {
                    stack.Push(folder.Name);
                }
                GetPath(tr.parent, stack);
            }

            var expressionMenu = menuRoot.AddMenu("Expressions");
            Stack<string> stack = new();
            Dictionary<ulong, MenuItem> nodes = new();

            foreach (var x in expressions)
            {
                string path;
                string name;
                if (x.Expression is IModEmoExpressionPattern)
                {
                    // default expression;
                    name = "Default";
                    path = $"{x.Expression.Name}{PathSegmentEnumerator.Separator}Default";
                }
                else
                {
                    name = x.Expression.Name;
                    stack.Clear();
                    GetPath(x.Expression.GameObject.transform, stack);
                    path = string.Join(PathSegmentEnumerator.Separator, stack);
                }

                MenuItem parent = expressionMenu;
                foreach(var seg in new PathSegmentEnumerator(path))
                {
                    if (seg.Length == path.Length)
                        break;

                    parent = nodes.GetOrAdd(seg, value => parent.AddMenu(Path.GetFileName(value).ToString()));
                }

                parent.AddToggle(name, ParameterNames.Expression.Index, x.Index);
            }
        }

        {
            var resetLayer = builder.AddLayer("[ModEmo] Reset Overrided Blendshapes");
            resetLayer.StateMachine.DefaultMotion = data.BlankClip;
            var idle = resetLayer.StateMachine.AddState("Idle");
            resetLayer.StateMachine.AddAnyStateTransition(idle).Equals(ParameterNames.Internal.BlendShapes.Reset, 0);

            var resetAllState = resetLayer.StateMachine.AddState("Reset All");
            resetLayer.StateMachine.AddAnyStateTransition(resetAllState).Equals(ParameterNames.Internal.BlendShapes.Reset, 1);
            var resetAll = resetAllState.AddAvatarParameterDriver();

            var blendShapeMenu = menuRoot.AddMenu("BlendShapes\n[LOCAL]");
            blendShapeMenu.AddButton("Reset", ParameterNames.Internal.BlendShapes.Reset, 1);
            string[] singleArray = new string[1];
            int resetLayerIdx = 2;
            foreach (var (key, values) in data.CategorizedBlendShapes)
            {
                var values2 = values.Where(data.UsageBlendShapeMap.ContainsKey).ToArray();

                MenuItem? menu = null;
                MenuItem? page = null;
                AvatarParameterDriverBuilder? reset = null;
                int pageCount = 1;
                foreach (var value in values2)
                {
                    if (menu == null)
                    {
                        menu = blendShapeMenu.AddMenu(key);
                        menu.AddButton("Reset", ParameterNames.Internal.BlendShapes.Reset, resetLayerIdx);
                        var state = resetLayer.StateMachine.AddState($"Reset {key}");
                        resetLayer.StateMachine.AddAnyStateTransition(state).Equals(ParameterNames.Internal.BlendShapes.Reset, resetLayerIdx);
                        reset = state.AddAvatarParameterDriver();
                        resetLayerIdx++;
                    }

                    if (values2.Length <= 8)
                    {
                        page = menu;
                    }
                    else if (page == null || page.transform.childCount >= 8)
                    {
                        page = menu.AddMenu($"Page {pageCount++}");
                    }
                    var name = $"{ParameterNames.Internal.BlendShapes.Prefix}{value}/Override";
                    page.AddRadialPuppet(value, name);
                    parameters.parameters.Add(new ParameterConfig() { nameOrPrefix = name, syncType = ParameterSyncType.Float, localOnly = true, saved = false });
                    reset?.Set(name, 0);
                    resetAll?.Set(name, 0);
                }
            }
        }
        
        var installer = menuRoot.gameObject.AddComponent<ModularAvatarMenuInstaller>();
    }

    private ref struct PathSegmentEnumerator
    {
        public const char Separator = '/';

        private readonly ReadOnlySpan<char> _source;
        private int _startCurrent;
        private int _endCurrent;
        private int _startNext;
        private bool flag;

        public PathSegmentEnumerator(ReadOnlySpan<char> source)
        {
            _source = source;
            _startCurrent = 0;
            _endCurrent = 0;
            _startNext = 0;
            flag = false;
        }

        public readonly PathSegmentEnumerator GetEnumerator() => this;

        public readonly ReadOnlySpan<char> Current => _source[.._endCurrent];

        public bool MoveNext()
        {
            if (flag)
                return false;

            int separatorIndex, separatorLength;
            separatorIndex = _source[_startNext..].IndexOf(Separator);
            separatorLength = 1;

            _startCurrent = _startNext;
            if (separatorIndex >= 0)
            {
                _endCurrent = _startCurrent + separatorIndex;
                _startNext = _endCurrent + separatorLength;
            }
            else
            {
                _startNext = _endCurrent = _source.Length;
                flag = true;
            }

            return true;
        }
    } 

    private static MenuItem CreateNewItem(this MenuItem parent, string name)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent.transform);
        var newItem = go.AddComponent<MenuItem>();
        newItem.MenuSource = SubmenuSource.Children;
        return newItem;
    }

    private static MenuItem WithSaved(this MenuItem menuItem, bool value)
    {
        menuItem.isSaved = value;
        return menuItem;
    }

    private static MenuItem WithSynced(this MenuItem menuItem, bool value)
    {
        menuItem.isSynced = value;
        return menuItem;
    }

    private static MenuItem WithDefault(this MenuItem menuItem, bool value)
    {
        menuItem.isDefault = value;
        return menuItem;
    }

    private static MenuItem AddMenu(this MenuItem parent, string name)
    {
        var newItem = parent.CreateNewItem(name);
        newItem.PortableControl.Type = PortableControlType.SubMenu;

        return newItem;
    }

    private static MenuItem AddToggle(this MenuItem parent, string name, string parameterName, float? value = null)
    {
        var newItem = parent.CreateNewItem(name);
        newItem.PortableControl.Type = PortableControlType.Toggle;
        newItem.PortableControl.Parameter = parameterName;
        newItem.PortableControl.Value = value ?? 1;
        return newItem;
    }

    private static MenuItem AddButton(this MenuItem parent, string name, string parameterName, float? value = null)
    {
        var newItem = parent.CreateNewItem(name);
        newItem.PortableControl.Type = PortableControlType.Button;
        newItem.PortableControl.Parameter = parameterName;
        newItem.PortableControl.Value = value ?? 1;
        return newItem;
    }

    private static MenuItem AddRadialPuppet(this MenuItem parent, string name, string parameterName)
    {
        var newItem = parent.CreateNewItem(name);
        newItem.PortableControl.Type = PortableControlType.RadialPuppet;
        newItem.PortableControl.SubParameters = ImmutableList.Create(parameterName);

        return newItem;
    }

    private static ref ParameterConfig GetOrAdd(this ModularAvatarParameters parameters, string name)
    {
        var list = parameters.parameters;
        foreach (ref var parameter in list.AsSpan())
        {
            if (parameter.nameOrPrefix == name)
                return ref parameter;
        }

        list.Add(new ParameterConfig() { nameOrPrefix = name });
        return ref list.AsSpan()[^1];
    }

    private static ref ParameterConfig GetOrAdd(this ModularAvatarParameters parameters, string name, Func<ParameterConfig, ParameterConfig> factory)
    {
        var list = parameters.parameters;
        foreach (ref var parameter in list.AsSpan())
        {
            if (parameter.nameOrPrefix == name)
                return ref parameter;
        }

        list.Add(factory(new ParameterConfig() { nameOrPrefix = name }));
        return ref list.AsSpan()[^1];
    }
}