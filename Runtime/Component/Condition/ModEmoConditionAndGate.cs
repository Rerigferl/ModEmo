
namespace Numeira
{
    internal sealed class ModEmoConditionAndGate : ModEmoConditionGate
    {
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