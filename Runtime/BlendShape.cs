namespace Numeira;

[Serializable]
internal struct BlendShape
{
    public string Name;
    public float Value;
    public bool Cancel;

    public override readonly int GetHashCode() => HashCode.Combine(Name, Value, Cancel);
}
