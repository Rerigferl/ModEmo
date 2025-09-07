
namespace Numeira
{
    [DisallowMultipleComponent]
    internal abstract class ModEmoConditionBase : ModEmoTagComponent, IModEmoConditionProvider
    {
        public abstract IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions();

        protected IModEmoConditionProvider[] Children => gameObject.GetComponentsInDirectChildren<IModEmoConditionProvider>();
    }


    internal interface IModEmoConditionProvider : IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>>
    {
        IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions();

        IEnumerator<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>>.GetEnumerator() => GetConditions().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}