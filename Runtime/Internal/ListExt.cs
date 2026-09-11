namespace Numeira;

internal static class ListExt
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Span<T> AsSpan<T>(this List<T> list)
    {
        var dummy = Unsafe.As<DummyList<T>>(list);
        return dummy.Array.AsSpan(0, dummy.Length);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void SetLength<T>(this List<T> list, int length)
    {
        Unsafe.As<DummyList<T>>(list).Length = length;
    }

    private sealed class DummyList<T>
    {
        public T[] Array = null!;
        public int Length;
    }
}
