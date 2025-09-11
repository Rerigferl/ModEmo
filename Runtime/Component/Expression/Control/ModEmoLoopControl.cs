
namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    [AddComponentMenu(ComponentMenuPrefix + "Loop Control")]
    internal sealed class ModEmoLoopControl : ModEmoTagComponent, IModEmoLoopControl
    {
        public bool IsLoop => true;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(IsLoop);
        }
    }

    internal interface IModEmoLoopControl : IModEmoComponent 
    {
        public bool IsLoop { get; }
    }
}