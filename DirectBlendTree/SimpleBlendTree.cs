namespace Numeira;

public sealed class SimpleBlendTree : BlendTreeBase
{
    public string BlendParameter { get; set; } = "";

    protected override void Build(BlendTree blendTree, float? threshold = null)
    {
        var tree = new BlendTree();
        tree.blendParameter = BlendParameter;
        tree.name = Name;
        tree.useAutomaticThresholds = false;

        foreach (var child in Children.Select((x, i) => (x.BlendTree, threshold: x.threshold ?? (Children.Count <= 1 ? 0 : (i / (Children.Count - 1))))).OrderBy(x => x.threshold))
        {
            child.BlendTree.Build(tree, child.threshold);
        }

        blendTree.AddChild(tree, threshold ?? 0);
    }
}
