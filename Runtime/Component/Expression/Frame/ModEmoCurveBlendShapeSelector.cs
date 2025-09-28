
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Curve BlendShape")]
    internal sealed class ModEmoCurveBlendShapeSelector : ModEmoTagComponent, IModEmoExpressionFrameProvider
    {
        public List<CurveBlendShape> BlendShapes = new();

        public IEnumerable<ExpressionFrame> GetFrames()
        {
            foreach(var blendShape in BlendShapes)
            {
                foreach(var key in blendShape.Value.keys)
                {
                    yield return ExpressionFrame.Create(this, key.time, new BlendShape() { Name = blendShape.Name, Cancel = blendShape.Cancel, Value = key.value });
                }
            }
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add(isActiveAndEnabled);
            foreach (var x in BlendShapes.AsSpan())
            {
                hashCode.Add(x);
            }
        }
    }

}