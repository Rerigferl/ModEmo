

namespace Numeira
{
    [DisallowMultipleComponent]
    internal abstract class ModEmoConditionBase : ModEmoTagComponent, IModEmoConditionProvider
    {
        public abstract IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions();

        protected IModEmoConditionProvider[] Children => this.GetComponentsInDirectChildren<IModEmoConditionProvider>();
    }


    internal interface IModEmoConditionProvider : IModEmoComponent, IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>>
    {
        IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions();

        IEnumerator<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>>.GetEnumerator() => GetConditions().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}