
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Blink Expression")]
    internal sealed class ModEmoBlinkExpression : ModEmoExpression, IModEmoLoopControl
    {
        public IEnumerable<CurveBlendShape> GetBlendShapes()
        {
            var writer = BlendShapeCurveWriter.Create();
            foreach(var x in this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: true))
            {
                x.CollectBlendShapes(writer);
            }

            return writer.Export();
        }

        private static readonly Curve.Keyframe[] SharedKeyframes = new Curve.Keyframe[6];

        public override IEnumerable<CurveBlendShape> BlendShapes
        {
            get
            {
                var blendShapes = GetBlendShapes();
                var keyframes = SharedKeyframes;

                foreach (var blendShape in blendShapes)
                {
                    var value = blendShape.Value.Evaluate(0);
                    keyframes[0] = new(0 / 60f, 0);
                    keyframes[1] = new(60 / 60f, 0);
                    keyframes[2] = new(65 / 60f, value);
                    keyframes[3] = new(67 / 60f, value);
                    keyframes[4] = new(80 / 60f, 0);
                    keyframes[5] = new(300 / 60f, 0);
                    yield return new CurveBlendShape(blendShape.Name, new(keyframes), blendShape.Cancel);
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