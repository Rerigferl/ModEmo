namespace Numeira;

public readonly struct BlendShapeInfo
{
    internal BlendShapeInfo(SkinnedMeshRenderer face, int index)
    {
        Value = face.GetBlendShapeWeight(index);
        Max = face.sharedMesh.GetBlendShapeFrameWeight(index, 0);
    }

    public BlendShapeInfo(float value, float max)
    {
        Value = value;
        Max = max;
    }

    public readonly float Value;
    public readonly float Max;
}