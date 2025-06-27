namespace Numeira;

public static class BlendTreeExt
{
    public static SimpleBlendTree AddBlendTree(this IBlendTree blendTree, string name = "", float? threshold = null)
        => blendTree.AddTo(new SimpleBlendTree() { Name = name }, threshold);

    public static DirectBlendTree AddDirectBlendTree(this IBlendTree blendTree, string name = "", float? threshold = null)
        => blendTree.AddTo(new DirectBlendTree() { Name = name }, threshold);

    public static MotionBranch AddMotion(this IBlendTree blendTree, Motion motion, float? threshold = null)
        => blendTree.AddTo(new MotionBranch(motion), threshold);

    public static ExponentialSmoothingBlendTree AddExponentialSmoothing(this IBlendTree blendTree, float? threshold = null)
        => blendTree.AddTo(new ExponentialSmoothingBlendTree(), threshold);

    private static T AddTo<T>(this IBlendTree blendTree, T value, float? threshold) where T : IBlendTree
    {
        blendTree.Append(value, threshold);
        return value;
    }

    public static T? Find<T>(this BlendTreeBase blendTree, string name) where T : IBlendTree
    {
        foreach (var (child, _) in blendTree.Children.AsSpan())
        {
            if (child is T x && x.Name == name)
            {
                return x;
            }
        }
        return default;
    }
}