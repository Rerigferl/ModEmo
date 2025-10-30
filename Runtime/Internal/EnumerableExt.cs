namespace Numeira;

internal static class EnumerableExt
{
    public static IEnumerable<(T Value, int Index)> Index<T>(this IEnumerable<T> enumerable)
        => enumerable.Select((x, i) => (x, i));

    public static IEnumerable<(T? Prev, T Current)> Pairwise<T>(this IEnumerable<T> enumerable)
    {
        T? prev = default;
        foreach(var x in enumerable)
        {
            yield return (prev, x);
            prev = x;
        }
    }

    public static T? MaxOrDefault<T>(this IEnumerable<T> enumerable)
    {
        if (!enumerable.Any())
            return default;

        if (typeof(T) == typeof(int))
        {
            var val = Unsafe.As<IEnumerable<int>>(enumerable).Max();
            return Unsafe.As<int, T>(ref val);
        }

        return enumerable.Max();
    }
}
