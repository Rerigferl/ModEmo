namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Composite Position")]
    internal sealed class ModEmoCompositePosition : ModEmoTagComponent, IPositionProviderComponent
    {
        public Vector2 Position;

        Vector2 IPositionProviderComponent.Position => Position;
    }
}