namespace Numeira;

internal interface IAnimationSourceComponent : IModEmoComponent, IOwnerComponent<IKeyframeWriterComponent>
{
    public void RegisterAnimations(IAnimationRegistry registry, in AnimationGeneratorOptions options);
}
