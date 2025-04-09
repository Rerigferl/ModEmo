namespace Numeira
{
    [AddComponentMenu(ComponentMenuPrefix + "Expression")]
    internal sealed class ModEmoExpression : ModEmoTagComponent, IModEmoExpression
    {
        public string Name = "";
        public bool DesyncWithObjectName = false;

        [SerializeReference]
        public IModEmoExpressionCondition[] Conditions = { };

        IEnumerable<IModEmoExpressionCondition> IModEmoExpression.Conditions => Conditions;

        private IEnumerable<BlendShape> BlendShapes => GetComponentsInChildren<ModEmoBlendShapesSelector>().SelectMany(x => x.BlendShapes);

        IEnumerable<BlendShape> IModEmoExpression.BlendShapes => BlendShapes;

        string IModEmoExpression.Name => DesyncWithObjectName ? Name : name;

#if UNITY_EDITOR
        void IModEmoExpression.Build(DirectBlendTree rootTree, Object assetContainer)
        {
            var clip = new AnimationClip();
            AssetDatabase.AddObjectToAsset(clip, assetContainer);
            foreach(var blendshape in BlendShapes)
            {
                AnimationUtility.SetEditorCurve(clip, AnimationUtils.CreateAAPBinding($"ModEmo/BlendShape/{blendshape.Name}"), AnimationCurve.Constant(0, 0, blendshape.Value));
            }
            rootTree.AddMotion(clip);
        }
#endif
    }

    internal interface IModEmoExpression
    {
        string Name { get; }

        IEnumerable<IModEmoExpressionCondition> Conditions { get; }

        IEnumerable<BlendShape> BlendShapes { get; }

#if UNITY_EDITOR
        void Build(DirectBlendTree rootTree, Object assetContainer);
#endif
    }
}