
namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    internal class ModEmoMotionTime : ModEmoTagComponent, IModEmoMotionTimeProvider
    {
        public string ParameterName = "";
        string? IModEmoMotionTimeProvider.ParameterName => ParameterName;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(ParameterName);
        }
    }

    internal interface IModEmoMotionTimeProvider
    {
        string? ParameterName { get; }
    }
}