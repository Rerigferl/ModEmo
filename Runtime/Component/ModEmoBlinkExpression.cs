
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Expression")]
    internal sealed class ModEmoBlinkExpression : ModEmoExpression
    {
        public BlendShape[] BlendShapes = { };

        protected override IEnumerable<IModEmoExpressionFrame> GetFrames()
        {
            var zero = BlendShapes.Select(x => x with { Value = 0 }).ToArray();
            yield return new ExpressionFrame(0 / 60f,  zero, this);
            yield return new ExpressionFrame(60 / 60f, zero, this);
            yield return new ExpressionFrame(65 / 60f, BlendShapes, this);
            yield return new ExpressionFrame(67 / 60f, BlendShapes, this);
            yield return new ExpressionFrame(80 / 60f, zero, this);
            yield return new ExpressionFrame(300 / 60f, zero, this);
        }

        protected override bool IsLoop() => true;
    }
}