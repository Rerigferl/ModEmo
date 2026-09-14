namespace Numeira;

internal readonly struct DefaultBlendshapeRecords
{
    private readonly HashSet<int> hashSet;

    public DefaultBlendshapeRecords()
    {
        hashSet = new();
    }

    public void Clear() => hashSet.Clear();

    public bool Contains(string name, BlendshapeControlType controlType, bool isFaceShape)
    {
        var hash = GetHashCode(name, controlType, isFaceShape);
        return hashSet.Contains(hash);
    }

    public bool Add(string name, float time, BlendshapeControlType controlType, bool isFaceShape)
    {
        if (time != 0)
            return false;

        var hash = GetHashCode(name, controlType, isFaceShape);
        return hashSet.Add(hash);
    }

    private static int GetHashCode(string name, BlendshapeControlType controlType, bool isFaceShape)
    {
        DeterministicHashCode hash = new();
        hash.Add(name);
        hash.Add(isFaceShape);
        hash.Add(controlType);
        return hash.ToHashCode();
    }
}
