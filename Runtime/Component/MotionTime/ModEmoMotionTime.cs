namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    internal abstract class ModEmoMotionTime : ModEmoTagComponent, IModEmoMotionTime
    {
        public abstract string? ParameterName { get; }
    }

    internal interface IModEmoMotionTime
    {
        string? ParameterName { get; }
    }
}