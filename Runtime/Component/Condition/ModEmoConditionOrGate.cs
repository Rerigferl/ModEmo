

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Condition Union")]
    internal sealed class ModEmoConditionOrGate : ModEmoConditionGate
    {
        protected override int UniqueId => 2;

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