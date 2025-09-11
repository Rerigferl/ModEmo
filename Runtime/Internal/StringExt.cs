using System.Runtime.InteropServices;

namespace Numeira;

internal static class StringExt
{
    public static int GetFarmHash(this string value) => (int)FarmHash.Hash32(MemoryMarshal.AsBytes<char>(value));
    public static ulong GetFarmHash64(this string value) => FarmHash.Hash64(MemoryMarshal.AsBytes<char>(value));
}
