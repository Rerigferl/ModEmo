

namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression Pattern")]
    internal sealed class ModEmoExpressionPattern : ModEmoExpressionFolder, IModEmoExpressionPattern, IModEmoComponent
    {
        public string Name = "";

        public IEnumerable<CurveBlendShape> BlendShapes
        {
            get
            {
                var writer = BlendShapeCurveWriter.Create();
                foreach (var myself in this.GetComponents<IModEmoBlendShapeProvider>())
                {
                    myself.CollectBlendShapes(writer);
                }
                foreach (var child in this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>().Where(x => x.Component.GetComponent<IModEmoExpression>() == null))
                {
                    child.CollectBlendShapes(writer);
                }
                return writer.Export();
            }
        }

        string IModEmoExpression.Name => !string.IsNullOrEmpty(Name) ? Name : name;

        ExpressionMode IModEmoExpression.Mode => ExpressionMode.Default;

        protected override void CalculateContentHash(ref HashCode hashCode)
        {
            hashCode.Add((this as IModEmoExpression).Name.GetFarmHash64());
            foreach(var x in this.GetComponentsInDirectChildren<IModEmoBlendShapeProvider>(includeSelf: true))
            {
                x.CalculateContentHash(ref hashCode);
            }
            base.CalculateContentHash(ref hashCode);
        }
    }

    internal interface IModEmoExpressionPattern : IModEmoExpression, IModEmoExpressionFolder
    {
    }
}