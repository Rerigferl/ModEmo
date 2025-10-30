
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Expression")]
    internal sealed class ModEmoBlinkExpression : ModEmoExpression, IModEmoLoopControl
    {
        public IEnumerable<BlendShape> GetBlendShapes()
        {
            foreach(var x in this.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>(includeSelf: true))
            {
                foreach(var frame in x.GetFrames())
                {
                    foreach (var blendShape in frame.GetBlendShapes())
                    {
                        yield return blendShape; 
                    }
                }
            }
        }

        public override IEnumerable<ExpressionFrame> Frames
        { 
            get
            {
                var blendShapes = GetBlendShapes();
                var zero = blendShapes.Select(x => x with { Value = 0 });
                yield return ExpressionFrame.Create(this, 0 / 60f, zero);
                yield return ExpressionFrame.Create(this, 60 / 60f, zero);
                yield return ExpressionFrame.Create(this, 65 / 60f, blendShapes);
                yield return ExpressionFrame.Create(this, 67 / 60f, blendShapes);
                yield return ExpressionFrame.Create(this, 80 / 60f, zero);
                yield return ExpressionFrame.Create(this, 300 / 60f, zero);
            }
        }

        private static readonly Keyframe[] SharedKeyframes = new Keyframe[6];

        public override IEnumerable<CurveBlendShape> BlendShapes
        {
            get
            {
                var blendShapes = GetBlendShapes();
                var keyframes = SharedKeyframes;

                foreach (var blendShape in blendShapes)
                {
                    keyframes[0] = new(0 / 60f, 0);
                    keyframes[1] = new(60 / 60f, 0);
                    keyframes[2] = new(65 / 60f, blendShape.Value);
                    keyframes[3] = new(67 / 60f, blendShape.Value);
                    keyframes[4] = new(80 / 60f, 0);
                    keyframes[5] = new(300 / 60f, 0);
                    yield return new CurveBlendShape(blendShape.Name, new AnimationCurve(keyframes), blendShape.Cancel);
                }
            }
        }

        public bool IsLoop => true;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var b in GetBlendShapes())
                hashCode.Add(b);

            base.CalculateContentHash(ref hashCode);
        }
    }
}