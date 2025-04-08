namespace Numeira;

internal static class Utils
{
    public static Span<T> AsSpan<T>(this List<T> list)
    {
        var tuple = Unsafe.As<Tuple<T[], int>>(list);
        return tuple.Item1.AsSpan(0, tuple.Item2);
    }
}