namespace Numeira
{
    internal sealed class ModEmoConditionAndGate : ModEmoConditionGate
    {
        protected override uint Gate(uint x, uint y) => x & y;
    }
}