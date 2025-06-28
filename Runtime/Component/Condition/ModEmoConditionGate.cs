namespace Numeira
{
    internal abstract class ModEmoConditionGate : ModEmoConditionBase
    {
        protected abstract uint Gate(uint x, uint y);

        protected IModEmoCondition[] Children => gameObject.GetComponentsInDirectChildren<IModEmoCondition>();

        public override ushort GetConditionMask()
        {
            ushort? result = null;
            foreach (var x in Children)
            {
                if (result is null)
                    result = x.GetConditionMask();
                else
                    result = (ushort)Gate(result.Value, x.GetConditionMask());
            }
            return result ?? 0;
        }
    }
}