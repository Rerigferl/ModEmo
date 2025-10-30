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
    public Curve Value;
    public bool Cancel;

    public CurveBlendShape(string name, Curve value, bool cancel = false)
    {
        Name = name;
        Value = value;
        Cancel = cancel;
    }

    public override readonly int GetHashCode() => HashCode.Combine(Name.GetFarmHash64(), Value, Cancel);
}