namespace Numeira;

public abstract class BlendTreeBase : IBlendTree
{
    public List<(IBlendTree BlendTree, float? threshold)> Children { get; } = new();

    public string Name { get; set; } = "";

    public virtual void Append(IBlendTree blendTree, float? threshold = null)
    {
        Children.Add((blendTree, threshold));
    }

    protected virtual void CorrectUsageAnimatorParameters(List<AnimatorParameter> result)
    {
        foreach(var (child, _) in Children)
        {
            child.CorrectUsageAnimatorParameters(result);
        }
    }

    protected abstract void Build(BlendTree blendTree, float? threshold = null);

    void IBlendTree.Build(BlendTree blendTree, float? threshold) => Build(blendTree, threshold);

    void IBlendTree.CorrectUsageAnimatorParameters(List<AnimatorParameter> result)
        => CorrectUsageAnimatorParameters(result);
}
