namespace Numeira;

[CustomPropertyDrawer(typeof(ExpressionSelectorAttribute))]
internal sealed class ExpressionSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);

        var obj = AvatarObjectReference.Get(property);
        if (obj != null && obj.GetComponent<IModEmoExpression>() is not { })
            obj = null;

        EditorGUI.BeginChangeCheck();
        obj = EditorGUI.ObjectField(position, label, obj, typeof(GameObject), property.serializedObject.targetObject) as GameObject;
        if (EditorGUI.EndChangeCheck())
        {
            if (obj == null || obj.GetComponent<IModEmoExpression>() is { })
            {
                AvatarObjectReference.Set(property, obj);
            }
        }

        EditorGUI.EndProperty();
    }
}

[CustomPropertyDrawer(typeof(AvatarObjectReference))]
internal sealed class AvatarObjectReferenceSelectorDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        label = EditorGUI.BeginProperty(position, label, property);

        var obj = AvatarObjectReference.Get(property)?.transform;

        EditorGUI.BeginChangeCheck();
        obj = EditorGUI.ObjectField(position, label, obj, typeof(Transform), property.serializedObject.targetObject) as Transform;
        if (EditorGUI.EndChangeCheck())
        {
            AvatarObjectReference.Set(property, obj?.gameObject ?? null);
        }

        EditorGUI.EndProperty();
    }
}