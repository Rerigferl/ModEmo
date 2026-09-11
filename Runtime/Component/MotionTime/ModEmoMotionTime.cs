
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Motion Time")]
    internal class ModEmoMotionTime : ModEmoTagComponent, IModEmoMotionTimeProvider
    {
        public string ParameterName = "";
        string? IModEmoMotionTimeProvider.ParameterName => ParameterName;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(ParameterName);
        }
    }

    internal interface IModEmoMotionTimeProvider : ISubComponent<IModEmoMotionTimeProvider>
    {
        string? ParameterName { get; }
    }
}