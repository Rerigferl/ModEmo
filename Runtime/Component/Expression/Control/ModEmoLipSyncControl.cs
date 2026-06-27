namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    [AddComponentMenu(ComponentMenuPrefix + "LipSync Control")]
    internal sealed class ModEmoLipSyncControl : ModEmoTagComponent, IModEmoLipSyncControl
    {
        private void OnEnable() { }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(enabled);
        }
    }
    internal interface IModEmoLipSyncControl : IModEmoExpressionControl
    {
    }
}