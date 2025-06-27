namespace Numeira
{
    internal sealed class ModEmoGestureWeightMotionTime : ModEmoMotionTime
    {
        public Hand Side = Hand.Left;

        public override string? ParameterName => Side switch
        {
            Hand.Left => ModEmoConstants.Parameters.Internal.Input.LeftWeight,
            Hand.Right => ModEmoConstants.Parameters.Internal.Input.RightWeight,
            _ => null,
        };
    }
}