namespace Numeira;

internal interface IAnimationSourceComponent : IModEmoComponent, ISubComponent<IAnimationSourceComponent>, IOwnerComponent<IKeyframeWriterComponent>
{
    public void RegisterAnimations(IAnimationRegistry registry, in AnimationGeneratorOptions options);
}
