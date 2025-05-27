namespace Numeira;

internal static class EnumerableExt
{
    public static IEnumerable<(T Value, int Index)> Index<T>(this IEnumerable<T> enumerable)
        => enumerable.Select((x, i) => (x, i));
}
