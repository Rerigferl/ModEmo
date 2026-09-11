namespace Numeira;

internal interface IPositionProviderComponent : IModEmoComponent, ISubComponent<IPositionProviderComponent>
{
    public Vector2 Position { get; }
}
