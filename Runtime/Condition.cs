namespace Numeira;

[Serializable]
internal struct Condition
{
    public Hand Hand;
    public Gesture Gesture;
    [Range(0, 1)]
    public float Weight;
}