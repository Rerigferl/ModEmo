namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Mouth Morph Cancel Control")]
    internal sealed class ModEmoMouthMorphCancelControl : ModEmoTagComponent, IModEmoMouthMorphCancelControl
    {
        private void OnEnable() { }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(enabled);
        }
    }

    internal interface IModEmoMouthMorphCancelControl : IModEmoExpressionControl
    {
    }
}