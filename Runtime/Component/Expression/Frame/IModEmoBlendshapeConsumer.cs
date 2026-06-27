namespace Numeira;

internal interface IModEmoBlendshapeConsumer : IModEmoComponent
{
    public IEnumerable<BlendShape> GetUsedBlendshapes();
}