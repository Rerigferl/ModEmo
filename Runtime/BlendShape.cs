namespace Numeira;

[Serializable]
internal struct BlendShape
{
    public string Name;
    public float Value;
    public bool Cancel;

    public override readonly int GetHashCode() => HashCode.Combine(Name.GetFarmHash64(), Value, Cancel);
}

[Serializable]
internal struct CurveBlendShape
{
    public string Name;
    public AnimationCurve Value;
    public bool Cancel;

    public CurveBlendShape(string name, AnimationCurve value, bool cancel = false)
    {
        Name = name;
        Value = value;
        Cancel = cancel;
    }

    public override readonly int GetHashCode() => HashCode.Combine(Name.GetFarmHash64(), Value, Cancel);
}

[Serializable]
internal struct ValueAnimationCurve
{
    private ValueKeyframe key1;
    private ValueKeyframe key2;
    private List<ValueKeyframe>? keys;
    private int length;

    public readonly int Length => length;

    public void AddKey(float time, float value)
    {
        AddKey(new ValueKeyframe(time, value));
    }

    public void AddKey(ValueKeyframe keyframe)
    {
        if (length == 0)
            key1 = keyframe;
        else if (length == 1)
            key2 = keyframe;
        else
            (keys ??= new()).Add(keyframe);
        length++;
    }



    private readonly ReadOnlySpan<ValueKeyframe> GetKeys(Span<ValueKeyframe> buffer)
    {
        Debug.Assert(buffer.Length >= length);

        if (length == 0)
            return default;

        if (length >= 1)
            buffer[0] = key1;

        if (length >= 2)
            buffer[1] = key2;

        if (length >= 3)
            keys!.AsSpan().CopyTo(buffer[2..]);

        return buffer;
    }
}

internal struct ValueKeyframe
{
    public float Time;

    public ValueKeyframe(float time, float value)
    {
        Time = time;
        Value = value;
        Tangent = default;
        TangentMode = default;
        WeightedMode = default;
        Weight = default;
    }

    public float Value;
    public (float In, float Out) Tangent;
    public int TangentMode;
    public int WeightedMode;
    public (float In, float Out) Weight;

    internal readonly bool IsEmpty => TangentMode == 0;
}