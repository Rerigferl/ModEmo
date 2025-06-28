namespace Numeira
{
    [RequireComponent(typeof(IModEmoExpressionFrame))]
    internal sealed class ModEmoExpressionBlinkController : ModEmoTagComponent, IModEmoExpressionBlinkController
    {
        public bool Enable = true;

        bool IModEmoExpressionBlinkController.Enable => Enable;
    }

    internal interface IModEmoExpressionBlinkController : IModEmoComponent
    {
        bool Enable { get; }
    }
}