namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Control")]
    internal sealed class ModEmoBlinkControl : ModEmoTagComponent, IModEmoBlinkControl
    {
        bool IModEmoBlinkControl.Enable => enabled;

        private void OnEnable() { }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(enabled);
        }
    }

    internal interface IModEmoBlinkControl : IModEmoComponent
    {
        bool Enable { get; }
    }
}