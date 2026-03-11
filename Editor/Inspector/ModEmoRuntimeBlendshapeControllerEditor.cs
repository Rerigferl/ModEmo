using System.Text.RegularExpressions;

namespace Numeira;

[CustomEditor(typeof(ModEmoRuntimeBlendshapeController))]
internal sealed class ModEmoRuntimeBlendshapeControllerEditor : Editor
{
    private SerializedProperty? SyncProperty;
    private SerializedProperty? BlacklistProperty;

    [SerializeField]
    private string[]? targetBlendshapes;

    private SerializedObject? so;

    public void OnEnable()
    {
        SyncProperty = serializedObject.FindProperty("Sync");
        BlacklistProperty = serializedObject.FindProperty("Blacklist");

        UpdateTargetBlendshapes();
        so = new(this);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        EditorGUILayout.PropertyField(SyncProperty);
        EditorGUILayout.PropertyField(BlacklistProperty);
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(so!.FindProperty("targetBlendshapes"));
        EditorGUI.EndDisabledGroup();
    }

    private void UpdateTargetBlendshapes()
    {
        var target = this.target as IModEmoRuntimeBlendshapeController;
        if (target == null)
            return;
        if (target.Component.GetComponentInParent<ModEmo>() is not { isActiveAndEnabled: true } root)
            return;

        HashSet<string> items = new();

        foreach(var expression in root.ExportExpressions().SelectMany(x => x))
        {
            foreach(var x in expression.Component.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: true))
            {
                foreach(var y in x.GetBlendShapes())
                {
                    items.Add(y.Name);
                }
            }
        }
        List<string>? remove = null;
        foreach (var pattern in target.Blacklist)
        {
            if (string.IsNullOrEmpty(pattern)) continue;
            var regex = new Regex(pattern, RegexOptions.CultureInvariant);

            foreach (var x in items)
            {
                if (regex.IsMatch(x))
                    (remove ??= new()).Add(x);
            }
        }
        if (remove is not null)
        {
            foreach (var x in remove.AsSpan())
            {
                items.Remove(x);
            }
        }

        foreach (var x in target.Component.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: true))
        {
            foreach (var blendShape in x.GetBlendShapes())
            {
                items.Add(blendShape.Name);
            }
        }
        targetBlendshapes = items.ToArray();
    }
}