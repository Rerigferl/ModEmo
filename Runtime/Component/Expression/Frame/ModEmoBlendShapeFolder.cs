
namespace Numeira
{
    internal class ModEmoBlendShapeFolder : ModEmoTagComponent, IModEmoBlendShapeProvider
    {
        protected virtual bool IncludeSelf => false;

        public IModEmoBlendShapeProvider[] Children => this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: IncludeSelf);

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