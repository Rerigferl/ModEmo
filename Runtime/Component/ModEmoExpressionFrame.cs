namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Frame")]
    internal sealed class ModEmoExpressionFrame : ModEmoTagComponent, IModEmoExpressionFrame
    {
        public float Keyframe;
        public TimeScale TimeScale = TimeScale.Frame;

        float IModEmoExpressionFrame.Keyframe => TimeScale is TimeScale.Seconds ? Keyframe : Keyframe / 60f;

        IEnumerable<BlendShape> IModEmoExpressionFrame.BlendShapes => gameObject.GetComponentsInDirectChildren<ModEmoBlendShapesSelector>().SelectMany(x => x.BlendShapes);
    }

    internal enum TimeScale
    {
        Frame = 0,
        Seconds = 1,
    }

    internal interface IModEmoExpressionFrame
    {
        float Keyframe { get; }
        IEnumerable<BlendShape> BlendShapes { get; }
    }

    internal record class ExpressionFrame : IModEmoExpressionFrame
    {
        public ExpressionFrame(float keyframe, IEnumerable<BlendShape> blendShapes)
        {
            Keyframe = keyframe;
            BlendShapes = blendShapes;
        }

        public float Keyframe { get; }

        public IEnumerable<BlendShape> BlendShapes { get; }

        public void Deconstruct(out float keyFrame, out IEnumerable<BlendShape> blendShapes) => (keyFrame, blendShapes) = (Keyframe, BlendShapes);
    }
}