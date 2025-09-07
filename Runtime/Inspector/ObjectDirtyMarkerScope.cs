namespace Numeira;

internal readonly ref struct ObjectDirtyMarkerScope
{
    private readonly Object @object;
    public ObjectDirtyMarkerScope(Object obj)
    {
        @object = obj;
#if UNITY_EDITOR
        EditorGUI.BeginChangeCheck();
#endif
    }

    public void Dispose()
    {
        if (@object == null)
            return;

#if UNITY_EDITOR
        if (!EditorGUI.EndChangeCheck())
            return;

        EditorUtility.SetDirty(@object);
#endif
    }
}