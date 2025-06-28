namespace Numeira
{
    [RequireComponent(typeof(IModEmoExpressionFrame))]
    internal sealed class ModEmoExpressionBlinkControl : ModEmoTagComponent, IModEmoExpressionBlinkControl
    {
        public bool Enable = true;

        bool IModEmoExpressionBlinkControl.Enable => Enable;
    }

    internal interface IModEmoExpressionBlinkControl : IModEmoComponent
    {
        bool Enable { get; }
    }
}