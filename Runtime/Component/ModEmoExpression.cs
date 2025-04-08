namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    internal sealed class ModEmoExpression : ModEmoTagComponent, IModEmoExpression
    {
        public string Name = "";
        public bool DesyncWithObjectName = false;

        public Condition[] Conditions = { };

        IEnumerable<Condition> IModEmoExpression.Conditions => Conditions;

        IEnumerable<BlendShape> IModEmoExpression.BlendShapes
        {
            get
            {
                var selectors = GetComponentsInChildren<ModEmoBlendShapesSelector>();
                return selectors.SelectMany(x => x.BlendShapes);
            }
        }

        string IModEmoExpression.Name => DesyncWithObjectName ? Name : name;
    }

    internal interface IModEmoExpression
    {
        string Name { get; }

        IEnumerable<Condition> Conditions { get; }

        IEnumerable<BlendShape> BlendShapes { get; }
    }
}