

namespace Numeira
{
    internal sealed class ModEmoConditionNotGate : ModEmoConditionGate
    {
        public override IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            foreach(var child in Children)
            {
                foreach(var group in child.GetConditions())
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