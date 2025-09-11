namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    [AddComponentMenu(ComponentMenuPrefix + "LipSync Control")]
    internal sealed class ModEmoLipSyncControl : ModEmoTagComponent, IModEmoLipSyncConttrol
    {
        public bool Enable = true;

        bool IModEmoLipSyncConttrol.Enable => Enable;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(Enable);
        }
    }

    internal interface IModEmoLipSyncConttrol : IModEmoComponent
    {
        public bool Enable { get; }
    }
}