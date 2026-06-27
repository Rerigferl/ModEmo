namespace Numeira;

internal interface IModEmoBlendShapeConsumer : IModEmoComponent
{
    public IEnumerable<BlendShape> GetUsageBlendshapes();
}