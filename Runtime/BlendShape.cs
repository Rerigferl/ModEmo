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

    public override readonly int GetHashCode() => HashCode.Combine(Name.GetFarmHash64(), Value, Cancel);
}