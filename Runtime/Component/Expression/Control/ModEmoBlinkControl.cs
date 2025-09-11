namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Control")]
    internal sealed class ModEmoBlinkControl : ModEmoTagComponent, IModEmoBlinkControl
    {
        public bool Enable = true;

        bool IModEmoBlinkControl.Enable => Enable;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(Enable);
        }
    }

    internal interface IModEmoBlinkControl : IModEmoComponent
    {
        bool Enable { get; }
    }
}