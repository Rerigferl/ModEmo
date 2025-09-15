using System.Collections.Immutable;

namespace Numeira;

[CustomEditor(typeof(ModEmoBlendShapeSelector), true)]
internal sealed class ModEmoBlendShapeSelectorEditor : Editor
{
    private SerializedProperty blendShapesProperty = null!;

    private ModEmoBlendShapeSelector Component => (target as ModEmoBlendShapeSelector)!;

    private ModEmo? Root => Component.GetComponentInParent<ModEmo>(true);

    private static List<bool> categoryOpenStatus = new();

    private List<KeyValuePair<string, List<string>>> CategorizedBlendShapes = null!;

    private ImmutableDictionary<string, BlendShapeInfo> BlendShapes = null!;


    public void OnEnable()
    {
        blendShapesProperty = serializedObject.FindProperty("BlendShapes");
        if (Root == null)
            return;

        (CategorizedBlendShapes, BlendShapes) = ModEmoData.GetCategorizedBlendShapes(Root) ?? default;

        categoryOpenStatus.Capacity = Math.Max(categoryOpenStatus.Capacity, CategorizedBlendShapes.Count);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical();

        EditorGUILayout.PropertyField(blendShapesProperty, new GUIContent("BlendShapes"));

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(GUILayout.Width(200));

        if (Root == null || CategorizedBlendShapes is null)
            return;

        var categories = CategorizedBlendShapes.AsSpan();
        string? preview = null;
        for (int i = 0; i < categories.Length; i++)
        {
            var category = categories[i];
            using var scope = new ShurikenHeaderGroupScope(ref Unsafe.As<Tuple<bool[], int>>(categoryOpenStatus).Item1.AsSpan()[i], category.Key, menuCallback: menu => MenuCallback(menu, category));
            if (!scope.IsOpened)
                continue;

            foreach (var x in category.Value.AsSpan())
            {
                var rect = EditorGUILayout.GetControlRect(false, EditorGUIUtility.singleLineHeight * 1.25f);

                if (rect.Contains(Event.current.mousePosition))
                {
                    preview ??= x;
                }

                if (GUI.Button(rect, x))
                {
                    var blendShapeValue = BlendShapes.TryGetValue(x, out var value) ? value.Max : 100;
                    Undo.RecordObject(Component, "Modify BlendShapes");
                    if (Event.current.button == 1)
                    {
                        Component.BlendShapes.RemoveAll(y => y.Name == x);
                    }
                    else if (Event.current.shift)
                    {
                        Component.BlendShapes.Add(new() { Name = x, Cancel = true, Value = blendShapeValue });
                    }
                    else
                    {
                        Component.BlendShapes.Add(new() { Name = x, Cancel = false, Value = blendShapeValue });
                    }
                }
            }
        }

        ExpressionPreview.TemporaryPreviewBlendShape.Value = preview;

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        serializedObject.ApplyModifiedProperties();
    }

    private void MenuCallback(GenericMenu menu, KeyValuePair<string, List<string>> group)
    {
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
        EditorGUI.ToggleLeft(toggleRect, GUIContent.none, cancelProp.boolValue);
        if (EditorGUI.EndChangeCheck())
        {
            cancelProp.boolValue = cancelValue;
        }

        EditorGUI.EndProperty();
        
    }
}