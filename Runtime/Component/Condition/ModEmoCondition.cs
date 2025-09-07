


namespace Numeira
{
    [DisallowMultipleComponent]
    internal sealed class ModEmoCondition : ModEmoConditionBase
    {
        public AnimatorParameterCondition[] Parameters = Array.Empty<AnimatorParameterCondition>();

        public override IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            yield return Group.Create(this, x => x.Parameters);
        }
    }
}