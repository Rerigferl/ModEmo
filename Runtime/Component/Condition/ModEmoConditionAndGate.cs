
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Condition Combiner")]
    internal sealed class ModEmoConditionAndGate : ModEmoConditionGate
    {
        protected override int UniqueId => 1;

        public override IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            yield return Group.Create(this, Factory);

            static IEnumerable<AnimatorParameterCondition> Factory(ModEmoConditionAndGate @this)
            {
                foreach (var child in @this.Children)
                {
                    foreach (var group in child.GetConditions())
                    {
                        foreach (var item in group)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }
}