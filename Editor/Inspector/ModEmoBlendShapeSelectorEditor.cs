using System.Collections.Immutable;
using System.Text.RegularExpressions;
using UnityEditorInternal;

namespace Numeira;

[CustomEditor(typeof(ModEmoBlendShapeSelector), true)]
[CanEditMultipleObjects]
internal sealed class ModEmoBlendShapeSelectorEditor : Editor
{
    private SerializedProperty blendShapesProperty = null!;

    private ModEmoBlendShapeSelector Component => (target as ModEmoBlendShapeSelector)!;

    private ModEmo? Root => Component.GetComponentInParent<ModEmo>(true);
    private FaceInfo? faceInfo;

    private bool[] categoryOpenStatus = null!;
    private Vector2[] categoryScrolls = null!;

    private BlendShapeList? blendShapeList;

    private readonly List<FaceInfo.BlendshapeInfo> temporaryItemStack = new();

    private string searchText = string.Empty;
    private Regex? searchTextRegEx;

    private bool isExpressionChild = false;

    internal Guid Identifier;
    private static ulong previewingControlId = 0;
    private static Guid? previewingIdentifier = null;

    private static bool IsExpressionChild(ModEmoBlendShapeSelector selector)
    {
        var t = selector.transform;
        while (t != null)
        {
            if (t.GetComponent<IModEmoExpression>() != null)
                return true;

            if (t != selector.transform && t.GetComponent<IModEmoBlendShapeFolder>() == null)
                return false;

            t = t.parent;
        }
        return false;
    }

    public void OnEnable()
    {
        Identifier = Guid.NewGuid();

        blendShapesProperty = serializedObject.FindProperty("BlendShapes");
        blendShapeList = new(serializedObject, blendShapesProperty);

        if (Root == null)
            return;

        faceInfo = new(Root);

        categoryOpenStatus = new bool[faceInfo.GroupedBlendShapes.Count];
        categoryScrolls = new Vector2[faceInfo.GroupedBlendShapes.Count];

        isExpressionChild = IsExpressionChild(Component);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();

        //isSettingsOpening = EditorGUILayout.BeginFoldoutHeaderGroup(isSettingsOpening, "Settings");
        //EditorGUILayout.EndFoldoutHeaderGroup();
        if (isExpressionChild)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Keyframe"));
            EditorGUILayout.Separator();
        }

        //EditorGUILayout.PropertyField(blendShapesProperty, new GUIContent("BlendShapes"));
        blendShapeList?.DoLayoutList();

        EditorGUILayout.EndVertical();


        if (Root != null && faceInfo != null)
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(200));

            EditorGUI.BeginChangeCheck();
            ((GUIPosition)EditorGUILayout.GetControlRect()).SearchField("", ref searchText);
            if (EditorGUI.EndChangeCheck())
            {
                if (string.IsNullOrEmpty(searchText))
                    searchTextRegEx = null;
                else
                    searchTextRegEx = new Regex(searchText, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }

            float lineHeight = EditorGUIUtility.singleLineHeight * 1.25f;

            var content = new GUIContent();
            var stack = temporaryItemStack;

            int categoryIdx = -1;
            foreach(var (category, values) in faceInfo.GroupedBlendShapes.OrderBy(x => x.Value.Span.FirstOrDefault()?.Index ?? -1))
            {
                categoryIdx++;
                stack.Clear();
                var blendShapes = values.Span;

                foreach (var blendShape in blendShapes)
                {
                    if (searchTextRegEx is { } regex && !regex.IsMatch(blendShape.Name))
                        continue;
                    stack.Add(blendShape);
                }

                if (stack.Count == 0)
                    continue;

                using var scope = new ShurikenHeaderGroupScope(ref categoryOpenStatus.AsSpan()[categoryIdx], category, menuCallback: menu => MenuCallback(menu, values));
                if (!scope.IsOpened)
                    continue;

                bool needScroll = stack.Count > 16;

                var maxHeight = lineHeight * Math.Min(24, stack.Count);

                ref var scroll = ref categoryScrolls.AsSpan()[categoryIdx];

                if (needScroll)
                    scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(maxHeight));

                var items = stack.AsSpan();
                for (int i2 = 0; i2 < items.Length; i2++)
                {
                    var blendShapeName = items[i2].Name;
                    content.text = blendShapeName;
                    var id = blendShapeName.GetFarmHash64();

                    var rect = EditorGUILayout.GetControlRect(false, lineHeight);

                    float viewTop = needScroll ? scroll.y : 0;
                    float viewBottom = viewTop + maxHeight;
                    float itemTop = rect.y;
                    float itemBottom = rect.y + rect.height;
                    bool isVisible = !needScroll || (itemBottom > viewTop && itemTop < viewBottom);

                    if (isVisible)
                    {
                        if (Event.current.type == EventType.Repaint)
                        {
                            if (rect.Contains(Event.current.mousePosition))
                            {
                                ExpressionPreview.TemporaryPreviewBlendShape.Value = blendShapeName;
                                previewingControlId = id;
                                previewingIdentifier = Identifier;
                            }
                            else if (previewingIdentifier == Identifier && previewingControlId == id)
                            {
                                ExpressionPreview.TemporaryPreviewBlendShape.Value = null;
                                previewingControlId = 0;
                                previewingIdentifier = null;
                            }
                        }

                        if (GUI.Button(rect, blendShapeName))
                        {
                            var blendShapeValue = items[i2].Max;
                            Undo.RecordObject(Component, "Modify BlendShapes");
                            if (Event.current.shift)
                            {
                                Component.BlendShapes.RemoveAll(y => y.Name == blendShapeName);
                            }
                            else if (Event.current.button == 1)
                            {
                                Component.BlendShapes.Add(new() { Name = blendShapeName, Cancel = true, Value = blendShapeValue });
                            }
                            else
                            {
                                Component.BlendShapes.Add(new() { Name = blendShapeName, Cancel = false, Value = blendShapeValue });
                            }
                            EditorUtility.SetDirty(Component);
                        }
                    }
                }

                if (needScroll)
                    EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }


        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void MenuCallback(GenericMenu menu, ReadOnlyMemory<FaceInfo.BlendshapeInfo> group)
    {
        menu.AddItem(new("Add Existing Blendshapes"), false, () =>
        {
            Undo.RecordObject(Component, "Add Blendshapes");
            foreach (var item in group.Span)
            {
                if (item.Value == 0)
                    continue;
                Component.BlendShapes.Add(new() { Name = item.Name, Cancel = false, Value = item.Value });
            }
        });

        menu.AddItem(new("Add Cancel BlendShapes"), false, () =>
        {
            Undo.RecordObject(Component, "Add Cancel Blendshapes");
            foreach (var item in group.Span)
            {
                if (item.Value == 0)
                    continue;
                Component.BlendShapes.Add(new() { Name = item.Name, Cancel = true, Value = item.Max });
            }
        });
    }

    private sealed class BlendShapeList : ReorderableListWrapper
    {
        public BlendShapeList(SerializedObject serializedObject, SerializedProperty elements) : base(serializedObject, elements, "Blendshapes")
        {
        }

        public override bool DisplayRemove => true;

        protected override void OnItemGUI(Rect rect, int index, bool isActive, bool isFocused)
        {
            var position = (GUIPosition)rect;
            var (left, right) = position.HorizontalSeparate(position.Width - 32, 4);
            var property = Elements.GetArrayElementAtIndex(index);
            EditorGUI.PropertyField(left, property);
            if (GUI.Button(right.Center(new(24, 24)), EditorGUIUtility.TrIconContent("Toolbar Minus", "Remove selection from the list")))
            {
                RemoveIndicies.Add(index);
            }
        }
    }
}

[CustomPropertyDrawer(typeof(BlendShape))]
internal sealed class BlendShapeDataDrawer : PropertyDrawer
{
    private static bool Multiline => true;// EditorGUIUtility.currentViewWidth < 600;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        => Multiline ? EditorGUIUtility.singleLineHeight * 2.2f : EditorGUIUtility.singleLineHeight * 1.2f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var totalLineHeight = Multiline ? EditorGUIUtility.singleLineHeight * 2 : EditorGUIUtility.singleLineHeight;
        position.y += (position.height - totalLineHeight) / 2;
        position.height = totalLineHeight;
        EditorGUI.BeginProperty(position, label, property);

        GUIPosition labelRect, toggleRect, sliderRect;
        var toggleWidth = EditorStyles.toggle.CalcSize(GUIContent.none).x;
        if (Multiline)
        {
            var pos = new GUIPosition(position);
            labelRect = pos.SingleLine();
            var line2 = labelRect.NewLine();

            (sliderRect, toggleRect) = line2.HorizontalSeparate(line2.Width - toggleWidth, 4);
        }
        else
        {
            labelRect = new GUIPosition(position);
            labelRect.Width = EditorGUIUtility.labelWidth;

            toggleRect = new GUIPosition(position);
            toggleRect.Width = EditorStyles.toggle.CalcSize(GUIContent.none).x;

            sliderRect = new GUIPosition(position);
            sliderRect.Width -= labelRect.Width + 8 + toggleRect.Width;
            sliderRect.X += labelRect.Width + 4;
            toggleRect.X = sliderRect.X + sliderRect.Width + 4;

        }

        labelRect.TextField("", property.FindPropertyRelative("Name"), "BlendShape");

        EditorGUI.Slider(sliderRect, property.FindPropertyRelative("Value"), 0, 100, "");

        var cancelProp = property.FindPropertyRelative("Cancel");
        bool cancelValue = cancelProp.boolValue;
        EditorGUI.BeginChangeCheck();
        cancelValue = EditorGUI.ToggleLeft(toggleRect, GUIContent.none, cancelValue);
        if (EditorGUI.EndChangeCheck())
        {
            cancelProp.boolValue = cancelValue;
        }

        EditorGUI.EndProperty();

    }
}

internal abstract class ReorderableListWrapper
{
    protected SerializedObject SerializedObject { get; }
    protected SerializedProperty Elements { get; }

    public string Title { get; set; }

    private ReorderableList? list;
    protected List<int> RemoveIndicies { get; } = new();

    protected ReorderableList InnerList => list ??= InitializeList();

    public virtual bool DisplayAdd { get; } = true;
    public virtual bool DisplayRemove { get; } = true;

    public ReorderableListWrapper(SerializedObject serializedObject, SerializedProperty elements, string? title = null)
    {
        SerializedObject = serializedObject;
        Elements = elements;
        Title = title ?? elements.displayName;
    }

    protected virtual ReorderableList InitializeList()
    {
        return new ReorderableList(SerializedObject, Elements)
        {
            headerHeight = 0,
            displayAdd = DisplayAdd,
            displayRemove = DisplayRemove,
            draggable = true,
            drawElementCallback = OnItemGUI,
            elementHeightCallback = GetElementHeight,
        };
    }

    protected virtual float GetElementHeight(int index) => EditorGUI.GetPropertyHeight(Elements.GetArrayElementAtIndex(index));

    protected abstract void OnItemGUI(Rect rect, int index, bool isActive, bool isFocused);

    public void DoLayoutList()
    {
        list ??= InitializeList();
        var rect = (GUIPosition)EditorGUILayout.GetControlRect(false);
        bool hasMultiple = Elements.hasMultipleDifferentValues;

        if (!hasMultiple)
            EditorGUI.BeginProperty(rect, GUIContent.none, Elements);

        var (leftRect, rightRect) = rect.HorizontalSeparate(rect.Width - 48, 2);
        bool foldout = Elements.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(leftRect, Elements.isExpanded, Title);
        EditorGUI.EndFoldoutHeaderGroup();
        EditorGUI.PropertyField(rightRect, Elements.FindPropertyRelative("Array.size"), GUIContent.none);

        if (!hasMultiple)
            EditorGUI.EndProperty();

        try
        {
            if (!foldout)
                return;
            RemoveIndicies.Clear();
            list.DoLayoutList();

            if (RemoveIndicies.Count == 0)
                return;
            Undo.SetCurrentGroupName("Remove Items");
            var id = Undo.GetCurrentGroup();
            foreach (var index in RemoveIndicies.OrderByDescending(x => x))
            {
                Elements.DeleteArrayElementAtIndex(index);
            }
            Undo.CollapseUndoOperations(id);
        }
        finally
        {
        }
    }

    public virtual int AddItem()
    {
        int index = Elements.arraySize;
        Elements.InsertArrayElementAtIndex(index);

        return index;
    }
}

internal sealed class CategorizedBlendShapeComparer : IComparer<BlendShape>
{
    private readonly Dictionary<string, int> sortOrder;

    public CategorizedBlendShapeComparer(List<KeyValuePair<string, List<string>>> categorizedBlendshapeNames)
    {
        var dict = new Dictionary<string, int>();

        var span = categorizedBlendshapeNames.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            var kvp = span[i];
            foreach (var blendShapeName in kvp.Value.AsSpan())
            {
                dict.TryAdd(blendShapeName, i);
            }
        }

        sortOrder = dict;
    }

    public int Compare(BlendShape x, BlendShape y)
    {
        if (x.Name == null && y.Name == null) return 0;
        if (x.Name == null) return 1;
        if (y.Name == null) return -1;

        // 1. Cancelが有効になっていないほうを先頭に
        int cancelComparison = x.Cancel.CompareTo(y.Cancel);
        if (cancelComparison != 0)
        {
            return cancelComparison;
        }

        if (!sortOrder.TryGetValue(x.Name, out var x1))
            x1 = 0;

        if (!sortOrder.TryGetValue(y.Name, out var y1))
            y1 = 0;

        var categoryComparison = x1.CompareTo(y1);
        if (categoryComparison != 0)
        {
            return categoryComparison;
        }

        return x.Name.CompareTo(y.Name);
    }
}