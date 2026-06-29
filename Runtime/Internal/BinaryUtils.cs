using System.Buffers.Binary;

namespace Numeira;

internal static class BinaryUtils
{
    public static ReadOnlySpan<int> PopIndex(uint value, Span<int> buffer)
    {
        if (buffer.Length < sizeof(uint))
            return default;

        int count = 0;
        for (int i = 0; i < buffer.Length; i++)
        {
            var x = value & 1 << i;
            if (x != 0)
            {
                buffer[count++] = i;
            }
        }
        return buffer[..count];
    }
}
