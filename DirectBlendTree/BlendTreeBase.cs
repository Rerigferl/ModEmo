namespace Numeira;

public abstract class BlendTreeBase : IBlendTree
{
    public List<(IBlendTree BlendTree, float? threshold)> Children { get; } = new();

    public string Name { get; set; } = "";

    public virtual void Append(IBlendTree blendTree, float? threshold = null)
    {
        Children.Add((blendTree, threshold));
    }

    protected abstract void Build(BlendTree blendTree, float? threshold = null);

    void IBlendTree.Build(BlendTree blendTree, float? threshold) => Build(blendTree, threshold);
}
