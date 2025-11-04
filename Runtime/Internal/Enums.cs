namespace Numeira;

internal sealed class Enums<T> where T : struct, Enum
{
    public static Enums<T> Default { get; } = new();

    private Enums()
    {
        values = (T[])Enum.GetValues(typeof(T));
    }

    private readonly T[] values;

    public T this[int index] => values[index];
}
