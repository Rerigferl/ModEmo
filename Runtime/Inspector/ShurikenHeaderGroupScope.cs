namespace Numeira;
#if UNITY_EDITOR
internal readonly ref struct ShurikenHeaderGroupScope
{
    private static readonly GUIStyle InnerBoxStyle;

    private readonly bool NeedEndLayoutGroup;
    private readonly bool InsertSpaceToEnd;
    public readonly bool IsOpened;
    public readonly SerializedProperty? Property;

    static ShurikenHeaderGroupScope()
    {
        InnerBoxStyle = new("HelpBox")
        {
            margin = new RectOffset(),
            padding = new RectOffset(6, 6, 6, 6),
        };
    }

    public ShurikenHeaderGroupScope(SerializedProperty group, string title, bool drawToggle = true, bool insertSpaceToEnd = true, Action<GenericMenu>? menuCallback = null)
    {
        InsertSpaceToEnd = insertSpaceToEnd;

        IsOpened = Foldout(group, title, drawToggle, menuCallback);
        Property = group;
        NeedEndLayoutGroup = false;
        if (!group.isExpanded)
            return;
        Draw(out NeedEndLayoutGroup);

    }

    public ShurikenHeaderGroupScope(ref bool expanded, string title, bool insertSpaceToEnd = true)
    {
        InsertSpaceToEnd = insertSpaceToEnd;
        Property = null;
        NeedEndLayoutGroup = false;
        if (!(IsOpened = expanded = Foldout(title, expanded)))
            return;

        Draw(out NeedEndLayoutGroup);
    }

    private void Draw(out bool needEndLayoutGroup)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(EditorGUI.indentLevel * 15f);

        EditorGUILayout.BeginVertical(InnerBoxStyle);
        needEndLayoutGroup = true;
    }

    public void Dispose()
    {
        if (Property != null)
            Property.isExpanded = IsOpened;
        if (!NeedEndLayoutGroup)
            return;

        EditorGUILayout.EndVertical();
        GUILayout.EndHorizontal();
        if (InsertSpaceToEnd)
            EditorGUILayout.Space();
    }


    private static bool Foldout(GUIPosition position, SerializedObject so)
    {
        return false;
    }

    private static bool Foldout(SerializedProperty property, string title, bool drawToggle, Action<GenericMenu>? menuCallback = null)
    {
        var style = ShurikenTitle ??= new("ShurikenModuleTitle")
        {
            font = EditorStyles.label.font,
            border = new RectOffset(15, 7, 4, 4),
            fixedHeight = 22,
            contentOffset = new Vector2(20f, -2f),
            fontSize = 12
        };
        var rect = EditorGUILayout.GetControlRect(false, 20f, style);// GUILayoutUtility.GetRect(16f, 20f, style);
        rect = EditorGUI.IndentedRect(rect);

        if (drawToggle && property.FindPropertyRelative("Enable") is { } enableProp)
        {
            GUI.Box(rect, GUIContent.none, style);
            var r = rect;
            r.x += EditorStyles.foldout.CalcSize(GUIContent.none).x + 4;
            r.width = EditorStyles.toggle.CalcSize(GUIContent.none).x;
            EditorGUI.BeginChangeCheck();
            var v = EditorGUI.ToggleLeft(r, title, enableProp.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                enableProp.boolValue = v;
            }
            r.x += r.width + 0;
            r.width = EditorStyles.label.CalcSize(L10n.TempContent(title)).x;
            EditorGUI.LabelField(r, title);
        }
        else
        {
            GUI.Box(rect, title, style);
        }

        var e = Event.current;

        Rect? contextRect = null;
        if (menuCallback != null)
        {
            var style2 = EditorStyles.foldoutHeaderIcon;
            var width = 16f;

            var p = rect;
            p.x += p.width;
            p.x -= width;
            p.width = width;
            if (e.type == EventType.Repaint)
                style2.Draw(p, false, true, false, false);
            contextRect = p;
        }

        bool display = property.isExpanded;
        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            if (menuCallback != null && e.button == 1 || contextRect?.Contains(e.mousePosition) == true)
            {
                var menu = new GenericMenu();
                menuCallback?.Invoke(menu);
                menu.DropDown(new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0));
                e.Use();
            }
            else
            {
                display = !display;
                e.Use();
            }
        }

        return display;
    }

    private static bool Foldout(string title, bool display, Action<GenericMenu>? menuCallback = null)
    {
        var style = ShurikenTitle ??= new("ShurikenModuleTitle")
        {
            font = EditorStyles.label.font,
            border = new RectOffset(15, 7, 4, 4),
            fixedHeight = 22,
            contentOffset = new Vector2(20f, -2f),
            fontSize = 12
        };
        var rect = EditorGUILayout.GetControlRect(false, 20f, style);// GUILayoutUtility.GetRect(16f, 20f, style);
        rect = EditorGUI.IndentedRect(rect);
        GUI.Box(rect, title, style);

        var e = Event.current;

        Rect? contextRect = null;
        if (menuCallback != null)
        {
            var style2 = EditorStyles.foldoutHeaderIcon;
            var width = 16f;

            var p = rect;
            p.x += p.width;
            p.x -= width;
            p.width = width;
            if (e.type == EventType.Repaint)
                style2.Draw(p, false, true, false, false);
            contextRect = p;
        }


        var toggleRect = new Rect(rect.x + 4f, rect.y + 2f, 13f, 13f);
        if (e.type == EventType.Repaint)
        {
            EditorStyles.foldout.Draw(toggleRect, false, false, display, false);
        }

        if (e.type == EventType.MouseDown && rect.Contains(e.mousePosition))
        {
            if (menuCallback != null && e.button == 1 || contextRect?.Contains(e.mousePosition) == true)
            {
                var menu = new GenericMenu();
                menuCallback?.Invoke(menu);
                menu.DropDown(new Rect(e.mousePosition.x, e.mousePosition.y, 0, 0));
                e.Use();
            }
            else
            {
                display = !display;
                e.Use();
            }
        }

        return display;
    }

    private static GUIStyle? ShurikenTitle;
}
#endif