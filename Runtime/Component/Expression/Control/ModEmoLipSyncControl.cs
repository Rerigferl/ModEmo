namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    [AddComponentMenu(ComponentMenuPrefix + "LipSync Control")]
    internal sealed class ModEmoLipSyncControl : ModEmoTagComponent, IModEmoLipSyncConttrol
    {
        bool IModEmoLipSyncConttrol.Enable => enabled;

        private void OnEnable() { }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(enabled);
        }
    }

    internal interface IModEmoLipSyncConttrol : IModEmoComponent
    {
        public bool Enable { get; }
    }
}