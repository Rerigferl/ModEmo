namespace Numeira;

public sealed class ExponentialSmoothingBlendTree : SmoothingBlendTree
{
    public string SmoothAmountParameterName { get; set; } = "";

    public override void Build(BlendTree blendTree, float? threshold = null)
    {
        var tree = new BlendTree
        {
            blendParameter = SmoothAmountParameterName,
            name = Name
        };

        var a = new BlendTree()
        {
            blendParameter = InputParameterName,
            name = InputParameterName,
        };

        var b = new BlendTree()
        {
            blendParameter = OutputParameterName,
            name = OutputParameterName,
        };

        var zero = CreateAAPClip(OutputParameterName, 0);
        var one = CreateAAPClip(OutputParameterName, 1);

        a.AddChild(zero, 0);
        a.AddChild(one, 1);

        b.AddChild(zero, 0);
        b.AddChild(one, 1);

        tree.AddChild(a, 0);
        tree.AddChild(b, 1);

        blendTree.AddChild(tree, threshold ?? 0);
    }
}

public abstract class SmoothingBlendTree : IBlendTree
{
    public string Name { get; set; } = "";

    public string InputParameterName { get; set; } = "";
    public string OutputParameterName { get; set; } = "";


    public void Append(IBlendTree blendTree, float? threshold = null)
    { }


    protected static EditorCurveBinding CreateAAPBinding(string name)
        => new() { path = "", propertyName = name, type = typeof(Animator) };

    protected static AnimationClip CreateAAPClip(string name, float value)
    {
        var motion = new AnimationClip() { name = $"{name} {value}" };
        var bind = CreateAAPBinding(name);
        AnimationUtility.SetEditorCurve(motion, bind, AnimationCurve.Constant(0, 0, value));
        return motion;
    }

    public abstract void Build(BlendTree blendTree, float? threshold = null);
}
