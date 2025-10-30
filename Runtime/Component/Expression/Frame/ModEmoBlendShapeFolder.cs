
namespace Numeira
{
    internal sealed class ModEmoBlendShapeFolder : ModEmoTagComponent, IModEmoBlendShapeProvider
    {
        public IModEmoBlendShapeProvider[] Children => this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: false);

        public void CollectBlendShapes(in BlendShapeCurveWriter writer)
        {
            foreach(var x in Children)
            {
                x.CollectBlendShapes(writer); 
            }
        }

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            foreach(var x in Children)
            {
                x.CalculateContentHash(ref hashCode);
            }
        }
    }
}