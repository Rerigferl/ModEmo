namespace Numeira;

[Serializable]
internal struct Condition
{
    public Hand Hand;
    public Gesture Gesture;
    [Range(0, 1)]
    public float Weight;
}

[Serializable]
internal sealed class VRChatExpressionCondition : IModEmoExpressionCondition
{
    public Hand Hand;
    public Gesture Gesture;

    public IEnumerable<int> ToExpressionIndexes()
    {
        if (Hand is Hand.Right)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return GestureToIndex(Gesture, (Gesture)i);
            }
        }
        else if (Hand is Hand.Left)
        {
            for (int i = 0; i < 8; i++)
            {
                yield return GestureToIndex((Gesture)i, Gesture);
            }
        }
        else
        {
            yield return GestureToIndex(Gesture, Gesture);
        }
        yield break;
    }

    private int GestureToIndex(Gesture right, Gesture left)
    {
        return 1 + (int)(left) + (int)(right) * 8;
    }
}

internal interface IModEmoExpressionCondition
{
    IEnumerable<int> ToExpressionIndexes();
}