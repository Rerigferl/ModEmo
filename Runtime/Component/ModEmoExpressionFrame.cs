namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Frame")]
    internal sealed class ModEmoExpressionFrame : ModEmoTagComponent, IModEmoExpressionFrame
    {
        public float Keyframe;
        public TimeScale TimeScale = TimeScale.Frame;

        public IModEmoComponent? Publisher => this;

        float IModEmoExpressionFrame.Keyframe => TimeScale is TimeScale.Seconds ? Keyframe : Keyframe / 60f;

        IEnumerable<BlendShape> IModEmoExpressionFrame.BlendShapes => gameObject.GetComponentsInDirectChildren<IModEmoExpressionFrame>().SelectMany(x => x.BlendShapes);
    }

    internal enum TimeScale
    {
        Frame = 0,
        Seconds = 1,
    }

    internal interface IModEmoExpressionFrame : IModEmoComponent
    {
        float Keyframe { get; }
        IEnumerable<BlendShape> BlendShapes { get; }
        IModEmoComponent? Publisher { get; }
    }

    internal record class ExpressionFrame : IModEmoExpressionFrame
    {
        public ExpressionFrame(float keyframe, IEnumerable<BlendShape> blendShapes, IModEmoComponent? publisher = null)
        {
            Keyframe = keyframe;
            BlendShapes = blendShapes;
            Publisher = publisher;
        }

        public float Keyframe { get; }

        public IEnumerable<BlendShape> BlendShapes { get; }

        public IModEmoComponent? Publisher { get; }

        public Component? Component => Publisher as Component;

        public void Deconstruct(out float keyFrame, out IEnumerable<BlendShape> blendShapes) => (keyFrame, blendShapes) = (Keyframe, BlendShapes);

    }
}