
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Gesture Weight Motion Time")]
    internal sealed class ModEmoGestureWeightMotionTime : ModEmoTagComponent, IModEmoMotionTimeProvider
    {
        public Hand Side = Hand.Left;

        public string? ParameterName => Side switch
        {
            Hand.Left => ModEmoConstants.Parameters.Internal.Input.LeftWeight,
            Hand.Right => ModEmoConstants.Parameters.Internal.Input.RightWeight,
            _ => null,
        };

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(Side);
        }
    }
}