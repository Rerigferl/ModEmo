namespace Numeira;

internal static class ComponentExt
{
    public static T[] GetComponentsInDirectChildren<T>(this Component co, bool includeInactive = false, bool includeSelf = false)
        => GetComponentsInDirectChildren<T>(co.gameObject, includeInactive, includeSelf ? co : null);

    private static T[] GetComponentsInDirectChildren<T>(this GameObject go, bool includeInactive = false, Component? self = null)
    {
        var result = new List<T>();
        var temp = new List<T>(4);
        if (self != null)
        {
            if (includeInactive || go.activeInHierarchy)
            {
                go.GetComponents(temp);
                foreach(var x in temp.AsSpan())
                {
                    if (x is Component y && y.GetInstanceID() == self?.GetInstanceID())
                        continue;
                    result.Add(x);
                }
            }
        }
        foreach(var child in go)
        {
            if (!includeInactive && !child.activeInHierarchy)
                continue;

            child.GetComponents(temp);
            result.AddRange(temp);
        }
        return result.ToArray();
    }

    public static ReadOnlySpan<T> GetComponentsInDirectChildren<T>(this GameObject go, List<T> result, bool includeInactive = false)
    {
        result ??= new();
        result.Clear();
        var temp = new List<T>(4);
        foreach (var child in go)
        {
            if (!includeInactive && !child.activeInHierarchy)
                continue;

            child.GetComponents(temp);
            result.AddRange(temp);
        }
        return result.AsSpan();
    }
    public static T? GetComponentInDirectChildren<T>(this Component component, bool includeInactive = false, bool includeSelf = false) where T : class?
        => component.gameObject.GetComponentInDirectChildren<T>(includeInactive, includeSelf);

    public static T? GetComponentInDirectChildren<T>(this GameObject go, bool includeInactive = false, bool includeSelf = false) where T : class?
    {
        if (includeSelf && go.GetComponent<T>() is { } self)
        {
            return self;
        }

        foreach (var child in go)
        {
            if (!includeInactive && !child.activeInHierarchy)
                continue;

            if (child.GetComponent<T>() is { } x)
                return x;
        }

        return null;
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
#if UNITY_EDITOR
        foreach (var component in obj.GetComponents<T>())
        {
            var x = component as MonoBehaviour;
            var so = new SerializedObject(x);
            var script = so.FindProperty("m_Script");
            script.objectReferenceValue = null;
            so.ApplyModifiedProperties();
        }
        GameObjectUtility.RemoveMonoBehavioursWithMissingScript(obj);
        return;
        #endif
    }
}
