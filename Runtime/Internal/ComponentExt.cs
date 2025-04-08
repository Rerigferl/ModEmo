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

internal static class ListSingleton<T>
{
    public static readonly List<T> Value = new(8);
}

internal static class ListExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this List<T> list)
    {
        var tuple = Unsafe.As<Tuple<T[], int>>(list);
        return tuple.Item1.AsSpan(0, tuple.Item2);
    }
}