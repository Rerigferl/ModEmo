
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Motion Time")]
    internal sealed class ModEmoBlinkMotionTime : ModEmoTagComponent, IModEmoMotionTimeProvider
    {
        public string? ParameterName => ModEmoConstants.Parameters.Blink.Timer;
    }
}