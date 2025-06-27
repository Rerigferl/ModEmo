namespace Numeira;

internal static class ComponentExt
{
    public static T[] GetComponentsInDirectChildren<T>(this Component co, bool includeInactive = false)
        => GetComponentsInDirectChildren<T>(co.gameObject, includeInactive);

    public static T[] GetComponentsInDirectChildren<T>(this GameObject go, bool includeInactive = false)
    {
        var result = new List<T>();
        var temp = new List<T>(4);
        foreach(var child in go)
        {
            if (!includeInactive && !child.activeInHierarchy)
                continue;

            child.GetComponents(temp);
            result.AddRange(temp);
        }
        return result.ToArray();
    }

    public static T GetOrAddComponent<T>(this GameObject obj) where T : Component
    {
        if (obj == null)
            return default!;

        if (obj.TryGetComponent<T>(out var x)) 
            return x;
        x = obj.AddComponent<T>();
        return x;
    }

    public static void RemoveComponents<T>(this GameObject obj)
    {
        foreach(var component in obj.GetComponents<T>())
        {
            var x = component as MonoBehaviour;
            var so = new SerializedObject(x);
            var script = so.FindProperty("m_Script");
            script.objectReferenceValue = null;
            so.ApplyModifiedProperties();
        }
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
        return;
    }
}
