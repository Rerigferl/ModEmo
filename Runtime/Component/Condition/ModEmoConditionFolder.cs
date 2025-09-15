
namespace Numeira
{
    [DisallowMultipleComponent]
    [AddComponentMenu(ComponentMenuPrefix + "Condition Folder")]
    internal sealed class ModEmoConditionFolder : ModEmoTagComponent, IModEmoConditionProvider
    {
        public IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions() 
            => this.GetComponentsInDirectChildren<IModEmoConditionProvider>(includeSelf: true).SelectMany(x => x);

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach(var child in this.GetComponentsInDirectChildren<IModEmoConditionProvider>(includeSelf: true))
            {
                child.CalculateContentHash(ref hashCode);
            }
        }
    }
}