

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Condition Inverter")]
    internal sealed class ModEmoConditionNotGate : ModEmoConditionGate
    {
        protected override int UniqueId => 3;

        public override IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            foreach (var child in Children)
            {
                foreach (var group in child.GetConditions())
                {
                    yield return Group.Create(this, group.Select(x => x.Reverse()));
                }
            }
        }

#if UNITY_EDITOR

        [MenuItem("CONTEXT/ModEmoConditionNotGate/Test")]
        public static void Test(MenuCommand command)
        {

        }

#endif
    }
}