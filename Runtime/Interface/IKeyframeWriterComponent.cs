namespace Numeira;

internal interface IKeyframeWriterComponent : IModEmoComponent, ISubComponent<IKeyframeWriterComponent>
{
    public int BlendParameterIndex => 0;
    public void WriteKeyframes(IKeyframeWriterContext context, in AnimationGeneratorOptions options);
}

internal interface IBlendshapeWriterComponent : IKeyframeWriterComponent, ISubComponent<IBlendshapeWriterComponent>
{

}