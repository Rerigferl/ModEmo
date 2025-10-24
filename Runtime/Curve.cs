namespace Numeira;

internal sealed class Curve
{
    public List<Keyframe> Keys { get; } = new();

    public float Evaluate(float time)
    {
        throw new NotImplementedException();
    }

    public struct Keyframe
    {
        public float Time;
        public float Value;
        public (float In, float Out) Tangent;
        public (int Tangent, int Weighted) Mode;
        public (float In, float Out) Weight;
    }
}
