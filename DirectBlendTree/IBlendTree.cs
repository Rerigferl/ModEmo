namespace Numeira;

public interface IBlendTree
{
    void Append(IBlendTree blendTree, float? threshold = null);
    void Build(BlendTree blendTree, float? threshold = null);
    string Name { get; }
}
