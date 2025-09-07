

namespace Numeira
{
    internal sealed class ModEmoConditionOrGate : ModEmoConditionGate
    {
        public override IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            foreach(var child in Children)
            {
                foreach(var group in child.GetConditions())
                {
                    yield return Group.Create(this, group);
                }
            }
        }
    }
}