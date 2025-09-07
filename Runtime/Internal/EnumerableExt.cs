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
}
