
namespace Numeira
{
    internal abstract class ModEmoConditionGate : ModEmoConditionBase
    {
        protected abstract int UniqueId { get; }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(UniqueId);
            foreach (var child in Children)
            {
                child.CalculateContentHash(ref hashCode);
            }
        }
    }
}