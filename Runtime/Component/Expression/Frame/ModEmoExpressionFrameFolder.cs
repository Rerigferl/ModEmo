
namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Frame Folder")]
    internal class ModEmoExpressionFrameFolder : ModEmoTagComponent, IModEmoExpressionFrameProvider, IModEmoBlendShapeProvider
    {
        protected virtual bool IncludeSelf => false;

        public void CollectBlendShapes(in BlendShapeCurveWriter writer)
        {
            foreach(var x in this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: false))
            {
                x.CollectBlendShapes(writer);
            }
        }

        public IEnumerable<ExpressionFrame> GetFrames() => this.GetComponentsInDirectChildren<IModEmoExpressionFrameProvider>(includeSelf: IncludeSelf).SelectMany(x => x.GetFrames());

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach (var x in GetFrames())
            {
                hashCode.Add(x);
            }
        }
    }
}