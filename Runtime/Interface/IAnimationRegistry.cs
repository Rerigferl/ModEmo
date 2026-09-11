namespace Numeira;

internal interface IAnimationRegistry
{
    public IKeyframeWriterContext RegisterAnimation(in AnimationGeneratorOptions options, string? blendParameter = null) 
        => BlankContext.Instance;

    public ITwoAxisAnimationRegistryFactory RegisterTwoAxisAnimation(string blendParameterX, string blendParameterY)
        => BlankContext.Instance;

    public IKeyframeWriterContext RegisterMultipleConditionAnimation(in AnimationGeneratorOptions options, params string[] blendParameters) 
        => BlankContext.Instance;

    private sealed class BlankContext : IKeyframeWriterContext, IAnimationRegistry, ITwoAxisAnimationRegistryFactory
    {
        public static readonly BlankContext Instance = new();

        public void AddBlendshape(Transform target, string name, float time, float value)
        { }

        public void AddCancelBlendshape(Transform target, string name, float time, float value)
        { }

        public void AddRotation(Transform target, float time, Vector3 eularAngle, bool relative = true)
        { }

        public void SetAnimatorParameter<T>(string name, float time, T value)
        { }

        public void SetAvatarParameter<T>(string name, T value)
        { }

        public IAnimationRegistry GetRegistry(Vector2 position) => this;
    }
}

internal interface ITwoAxisAnimationRegistryFactory
{
    public IAnimationRegistry GetRegistry(Vector2 position);
}