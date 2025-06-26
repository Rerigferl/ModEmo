namespace Numeira
{
    internal sealed class ModEmoConditionOrGate : ModEmoConditionGate
    {
        protected override uint Gate(uint x, uint y) => x | y;
    }
}