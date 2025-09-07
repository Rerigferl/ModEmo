namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    internal sealed class ModEmoExpressionLoopControl : ModEmoTagComponent, IModEmoExpressionLoopControl
    {
        public bool IsLoop => true;
    }
    internal interface IModEmoExpressionLoopControl : IModEmoComponent 
    {
        public bool IsLoop { get; }
    }
}