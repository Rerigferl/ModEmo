namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Cancel Control")]
    internal sealed class ModEmoMouthMorphCancelControl : ModEmoTagComponent, IModEmoMouthMorphCancelControl
    {
        public bool Enable = false;

        bool IModEmoMouthMorphCancelControl.Enable => Enable;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(Enable);
        }
    }
    internal interface IModEmoMouthMorphCancelControl
    {
        public bool Enable { get; }
    }
}