

namespace Numeira;

internal abstract class ExpressionFrame : IGrouping<float, BlendShape>
{
    public float Time { get; }

    public object? Publisher { get; init; }

    public bool Blink => Publisher is not Component component || !(component.GetComponent<IModEmoBlinkControl>() is { } blinkCtrl) || blinkCtrl.Enable;

    protected ExpressionFrame(float time)
    {
        Time = time;
    }

    float IGrouping<float, BlendShape>.Key => Time;

    public abstract IEnumerable<BlendShape> GetBlendShapes();

    IEnumerator<BlendShape> IEnumerable<BlendShape>.GetEnumerator() => GetBlendShapes().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetBlendShapes().GetEnumerator();

    public static ExpressionFrame Create(object publisher, float time, BlendShape blendShape) => new SingleExpressionFrame(time, blendShape) { Publisher = publisher };
    public static ExpressionFrame Create(object publisher, float time, IEnumerable<BlendShape> blendShapes) => new MultiExpressionFrame(time, blendShapes) { Publisher = publisher };

    public override int GetHashCode()
    {
        HashCode hashCode = new();
        hashCode.Add(Time);
        foreach (var x in GetBlendShapes())
        {
            hashCode.Add(x);
        }
        return hashCode.ToHashCode();
    }
}

internal sealed class SingleExpressionFrame : ExpressionFrame
{
    public SingleExpressionFrame(float time, BlendShape blendShape) : base(time)
    {
        BlendShape = blendShape;
    }

    public BlendShape BlendShape { get; }

    public override IEnumerable<BlendShape> GetBlendShapes()
    {
        yield return BlendShape;
    }
}

internal sealed class MultiExpressionFrame : ExpressionFrame
{
    public MultiExpressionFrame(float time, IEnumerable<BlendShape> blendShapes) : base(time)
    {
        BlendShapes = blendShapes;
    }

    public IEnumerable<BlendShape> BlendShapes { get; }

    public override IEnumerable<BlendShape> GetBlendShapes() => BlendShapes;
}