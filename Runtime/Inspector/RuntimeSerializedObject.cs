namespace Numeira;

internal readonly struct RuntimeSerializedObject
{
#if UNITY_EDITOR
    private readonly SerializedObject serializedObject;

    public RuntimeSerializedObject(SerializedObject serializedObject)
    {
        this.serializedObject = serializedObject;
    }

    public SerializedProperty? this[string name] => FindProperty(name);

    public SerializedProperty? FindProperty(string name)
        => serializedObject.FindProperty(name);

    public SerializedObjectUpdateScope Update(bool undo = true) => new(serializedObject, undo);

    public bool ApplyModifiedProperties() => serializedObject.ApplyModifiedProperties();

    public static implicit operator SerializedObject(RuntimeSerializedObject value)
        => value.serializedObject;

    public readonly ref struct SerializedObjectUpdateScope
    {
        private readonly SerializedObject serializedObject;
        private readonly bool undo;
        public readonly bool Updated;

        public SerializedObjectUpdateScope(SerializedObject serializedObject, bool undo = true)
        {
            this.serializedObject = serializedObject;
            this.undo = undo;
            Updated = serializedObject.UpdateIfRequiredOrScript();
        }

        public void Dispose()
        {
            if (undo)
                serializedObject.ApplyModifiedProperties();
            else
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }
    }
#endif
}
