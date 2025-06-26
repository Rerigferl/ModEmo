namespace Numeira
{
    internal sealed class ModEmoVRChatCondition : ModEmoConditionBase
    {
        public Hand Hand;
        public Gesture Gesture;

        public override ushort GetConditionMask()
        {
            return Hand switch
            {
                Hand.Left => (ushort)(0b_0000_0000_0000_0001 << (int)Gesture),
                Hand.Right => (ushort)(0b_0000_0001_0000_0000 << (int)Gesture),
                _ => 0,
            };
        }
    }
}