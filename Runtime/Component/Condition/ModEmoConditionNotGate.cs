namespace Numeira
{
    internal sealed class ModEmoConditionNotGate : ModEmoConditionGate
    {
        public override ushort GetConditionMask()
        {
            return (ushort)~base.GetConditionMask();
        }

        protected override uint Gate(uint x, uint y) => x;


        [MenuItem("CONTEXT/ModEmoConditionNotGate/Test")]
        public static void Test(MenuCommand command)
        {
            Debug.LogError($"{Convert.ToString((long)((command.context as IModEmoCondition)?.GetConditionMask() ?? 0), 2),64}");
        }
    }
}