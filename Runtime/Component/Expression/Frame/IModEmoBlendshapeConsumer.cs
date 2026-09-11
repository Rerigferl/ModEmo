namespace Numeira;

[Obsolete]
internal interface IModEmoBlendshapeConsumer : IModEmoComponent
{
    public IEnumerable<BlendShape> GetUsedBlendshapes();
}