namespace Numeira;

internal struct BitFlags<T> where T : unmanaged
{
    private T value;

    public BitFlags(T value)
    {
        this.value = value;
    }

    public readonly bool IsDefault => EqualityComparer<T>.Default.Equals(value, default);

    public readonly int Count => Unsafe.SizeOf<T>() * 8;

    public bool this[int index]
    {
        get
        {
            if (Count < index)
                return false;

            return (Unsafe.As<T, ulong>(ref value) & 1u << index) != 0;
        }
        set
        {
            if (Count < index)
                return;

            if (value)
                Unsafe.As<T, ulong>(ref this.value) |= 1u << index;
            else
                Unsafe.As<T, ulong>(ref this.value) &= ~(1u << index);
        }
    }

    public readonly ReadOnlySpan<int> PopIndex(Span<int> buffer)
    {
        if (buffer.Length < Count)
            return default;

        int count = 0;
        int length = Math.Min(Count, buffer.Length);
        T v = this.value;
        ulong value = Unsafe.As<T, ulong>(ref v);
        for (int i = 0; i < length; i++)
        {
            var x = value & (1u << i);
            if (x != 0)
            {
                buffer[count++] = i;
            }
        }
        return buffer[..count];
    }

    public void Clear() => value = default;
}
