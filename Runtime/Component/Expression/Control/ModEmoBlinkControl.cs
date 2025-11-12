namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Control")]
    internal sealed class ModEmoBlinkControl : ModEmoTagComponent, IModEmoBlinkControl, IModEmoAnimationProvider
    {
        bool IModEmoBlinkControl.Enable => enabled;

        private void OnEnable() { }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(enabled);
        }

        public void WriteAnimation(IAnimationWriter writer, in AnimationWriterContext context)
        {
            writer.Write(AnimationBinding.Parameter(ModEmoConstants.Parameters.Blink.Value), 0, enabled ? 1 : 0);
        }
    }

    internal interface IModEmoBlinkControl : IModEmoComponent
    {
        bool Enable { get; }
    }
}