using System.Collections.Generic;

namespace Numeira;

internal static class ListExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this List<T> list)
    {
        var tuple = Unsafe.As<Tuple<T[], int>>(list);
        return tuple.Item1.AsSpan(0, tuple.Item2);
    }
}
