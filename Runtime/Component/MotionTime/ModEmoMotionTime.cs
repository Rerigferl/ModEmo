namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    internal class ModEmoMotionTime : ModEmoTagComponent, IModEmoMotionTimeProvider
    {
        public string ParameterName = "";
        string? IModEmoMotionTimeProvider.ParameterName => ParameterName;
    }

    internal interface IModEmoMotionTimeProvider
    {
        string? ParameterName { get; }
    }
}