namespace Numeira
{
    [RequireComponent(typeof(ModEmoExpression))]
    internal sealed class ModEmoExpressionLoopControl : ModEmoTagComponent, IModEmoExpressionLoopControl { }
    internal interface IModEmoExpressionLoopControl : IModEmoComponent { }
}