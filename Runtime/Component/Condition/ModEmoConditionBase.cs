namespace Numeira
{
    [DisallowMultipleComponent]
    internal abstract class ModEmoConditionBase : ModEmoTagComponent, IModEmoCondition
    {
        public abstract ushort GetConditionMask();
    }

    internal interface IModEmoCondition
    {
        public ushort GetConditionMask();
    }
}