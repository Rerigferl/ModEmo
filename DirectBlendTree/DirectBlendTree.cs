namespace Numeira;
public class DirectBlendTree : BlendTreeBase
{
    public string DirectBlendParameter { get; set; } = "1";

    public BlendTree Build(Object? assetContainer = null)
    {
        var tree = CreateDirectBlendTree();
        tree.name = $"{tree.name}(WD On)";
        void Recursive(BlendTree tree)
        {
            var children = tree.children;
            foreach (ref var x in children.AsSpan())
            {
                x.directBlendParameter = DirectBlendParameter;
                if (x.motion is BlendTree bt)
                {
                    bt.hideFlags |= HideFlags.HideInHierarchy;
                    if (assetContainer != null)
                        AssetDatabase.AddObjectToAsset(bt, assetContainer);

                    Recursive(bt);
                }
            }
            tree.children = children;
        }
        Recursive(tree);
        return tree;
    }

    protected override void Build(BlendTree blendTree, float? threshold = null)
    {
        blendTree.AddChild(CreateDirectBlendTree(), threshold ?? 0);
    }

    private BlendTree CreateDirectBlendTree()
    {
        var blendTree = new BlendTree();
        blendTree.name = Name;
        blendTree.blendType = BlendTreeType.Direct;
        SetNormalizedBlendValues(blendTree, false);
        foreach (var (child, _) in Children)
        {
            child.Build(blendTree);
        }
        return blendTree;
    }

    private static void SetNormalizedBlendValues(BlendTree blendTree, bool value)
    {
        using var so = new SerializedObject(blendTree);
        so.FindProperty("m_NormalizedBlendValues").boolValue = value;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    public IEnumerable<AnimatorParameter> GetAnimatorParameters()
    {
        var list = new List<AnimatorParameter>();
        GetAnimatorParameters(list);
        return list;
    }

    public void GetAnimatorParameters(List<AnimatorParameter> result)
    {
        CorrectUsageAnimatorParameters(result);
    }

    protected override void CorrectUsageAnimatorParameters(List<AnimatorParameter> result)
    {
        result.Add(new(DirectBlendParameter, 1f));
        base.CorrectUsageAnimatorParameters(result);
    }
}
