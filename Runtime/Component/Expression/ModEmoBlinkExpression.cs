
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Expression")]
    internal sealed class ModEmoBlinkExpression : ModEmoExpression, IModEmoLoopControl
    {
        public BlendShape[] BlendShapes = { };

        public override IEnumerable<ExpressionFrame> Frames
        { 
            get
            {
                var zero = BlendShapes.Select(x => x with { Value = 0 });
                yield return ExpressionFrame.Create(this, 0 / 60f, zero);
                yield return ExpressionFrame.Create(this, 60 / 60f, zero);
                yield return ExpressionFrame.Create(this, 65 / 60f, BlendShapes);
                yield return ExpressionFrame.Create(this, 67 / 60f, BlendShapes);
                yield return ExpressionFrame.Create(this, 80 / 60f, zero);
                yield return ExpressionFrame.Create(this, 300 / 60f, zero);
            }
        }

        public bool IsLoop => true;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var b in BlendShapes)
                hashCode.Add(b);

            base.CalculateContentHash(ref hashCode);
        }
    }
}