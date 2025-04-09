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
}
