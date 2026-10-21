namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Expression")]
    internal sealed class ModEmoBlinkExpressionV2 : ModEmoBlinkExpression
    {
        protected override bool UseBlinkTimer => true;
    }
}