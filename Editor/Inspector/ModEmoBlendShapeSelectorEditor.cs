using System.Collections.Immutable;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;

namespace Numeira;

[CustomEditor(typeof(ModEmoBlendShapeSelector), true)]
[CanEditMultipleObjects]
internal sealed class ModEmoBlendShapeSelectorEditor : Editor
{
    private SerializedProperty blendShapesProperty = null!;

    private ModEmoBlendShapeSelector Component => (target as ModEmoBlendShapeSelector)!;

    private ModEmo? Root => Component.GetComponentInParent<ModEmo>(true);

    private bool[] categoryOpenStatus = null!;
    private Vector2[] categoryScrolls = null!;

    private List<KeyValuePair<string, List<string>>> CategorizedBlendShapes = null!;

    private ImmutableDictionary<string, BlendShapeInfo> BlendShapes = null!;
    private BlendShapeList? blendShapeList;

    private readonly List<string> temporaryItemStack = new();

    private string searchText = string.Empty;
    private Regex? searchTextRegEx;

    public void OnEnable()
    {
        blendShapesProperty = serializedObject.FindProperty("BlendShapes");
        blendShapeList = new(serializedObject, blendShapesProperty);

        if (Root == null)
            return;

        (CategorizedBlendShapes, BlendShapes) = ModEmoData.GetCategorizedBlendShapes(Root) ?? default;

        categoryOpenStatus = new bool[CategorizedBlendShapes.Count];
        categoryScrolls = new Vector2[CategorizedBlendShapes.Count];
    }

    private static int previewingControlId = 0;
    private static bool isSettingsOpening;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();

        //isSettingsOpening = EditorGUILayout.BeginFoldoutHeaderGroup(isSettingsOpening, "Settings");
        //EditorGUILayout.EndFoldoutHeaderGroup();
        //if (isSettingsOpening)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Keyframe"));
            EditorGUILayout.Separator();
        }

        //EditorGUILayout.PropertyField(blendShapesProperty, new GUIContent("BlendShapes"));
        blendShapeList?.DoLayoutList();

        EditorGUILayout.EndVertical();


        if (Root != null && CategorizedBlendShapes is not null)
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(200));

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

            var categories = CategorizedBlendShapes.AsSpan();
            var content = new GUIContent();
            var stack = temporaryItemStack;
            for (int i = 0; i < categories.Length; i++)
            {
                stack.Clear();
                var category = categories[i];
                var blendShapes = category.Value.AsSpan();

                foreach (var blendShape in blendShapes)
                {
                    if (searchTextRegEx is { } regex && !regex.IsMatch(blendShape))
                        continue;
                    stack.Add(blendShape);
                }

                if (stack.Count == 0)
                    continue;

                using var scope = new ShurikenHeaderGroupScope(ref categoryOpenStatus.AsSpan()[i], category.Key, menuCallback: menu => MenuCallback(menu, category));
                if (!scope.IsOpened)
                    continue;

                bool needScroll = stack.Count > 16;

                var maxHeight = lineHeight * Math.Min(24, stack.Count);

                ref var scroll = ref categoryScrolls.AsSpan()[i];

                if (needScroll)
                    scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MaxHeight(maxHeight));

                foreach (var blendShapeName in stack.AsSpan())
                {
                    content.text = blendShapeName;
                    var id = GUIUtility.GetControlID(content, FocusType.Passive);
                    var rect = EditorGUILayout.GetControlRect(false, lineHeight);

                    if (rect.Contains(Event.current.mousePosition))
                    {
                        ExpressionPreview.TemporaryPreviewBlendShape.Value = blendShapeName;
                        previewingControlId = id;
                    }
                    else if (previewingControlId == id)
                    {
                        ExpressionPreview.TemporaryPreviewBlendShape.Value = null;
                        previewingControlId = 0;
                    }

                    if (GUI.Button(rect, blendShapeName))
                    {
                        var blendShapeValue = BlendShapes.TryGetValue(blendShapeName, out var value) ? value.Max : 100;
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

    private void MenuCallback(GenericMenu menu, KeyValuePair<string, List<string>> group)
    {
        menu.AddItem(new("Add Existing Blendshapes"), false, () =>
        {
            Undo.RecordObject(Component, "Add Blendshapes");
            foreach (var item in group.Value)
            {
                if (!BlendShapes.TryGetValue(item, out var blendShape) || blendShape.Value == 0)
                    continue;
                Component.BlendShapes.Add(new() { Name = item, Cancel = false, Value = blendShape.Value });
            }
        });

        menu.AddItem(new("Add Cancel BlendShapes"), false, () =>
        {
            Undo.RecordObject(Component, "Add Cancel Blendshapes");
            foreach(var item in group.Value)
            {
                if (!BlendShapes.TryGetValue(item, out var blendShape) || blendShape.Value == 0)
                    continue;
                Component.BlendShapes.Add(new() { Name = item, Cancel = true, Value = blendShape.Max });
            }
        });
    }

    private sealed class BlendShapeList : ReorderableListWrapper
    {
        public BlendShapeList(SerializedObject serializedObject, SerializedProperty elements) : base(serializedObject, elements, "Blendshapes")
        {
        }

        public override bool DisplayRemove => false;

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
        var rect = (GUIPosition)EditorGUILayout.GetControlRect();
        bool hasMultiple = Elements.hasMultipleDifferentValues;

        if (!hasMultiple)
            EditorGUI.BeginProperty(rect, GUIContent.none, Elements);

        var (leftRect, rightRect) = rect.HorizontalSeparate(rect.Width - 48, 2);
        bool foldout = Elements.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(leftRect, Elements.isExpanded, Title);
        EditorGUILayout.EndFoldoutHeaderGroup();
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
}