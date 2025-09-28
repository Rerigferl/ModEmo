namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Cancel Control")]
    internal sealed class ModEmoMouthMorphCancelControl : ModEmoTagComponent, IModEmoMouthMorphCancelControl
    {
        bool IModEmoMouthMorphCancelControl.Enable => enabled;

        private void OnEnable() { }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(enabled);
        }
    }
    internal interface IModEmoMouthMorphCancelControl
    {
        public bool Enable { get; }
    }
}