namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Condition")]
    [CanEditMultipleObjects]
    internal sealed class ModEmoCondition : ModEmoConditionBase
    {
        public List<AnimatorParameterCondition> Parameters = new();

        public override IEnumerable<IGrouping<IModEmoConditionProvider, AnimatorParameterCondition>> GetConditions()
        {
            yield return Group.Create(this, x => x.Parameters);
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach(var x in Parameters.AsSpan())
            {
                hashCode.Add(x);
            }
        }
    }
}