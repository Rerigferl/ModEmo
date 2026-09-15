namespace Numeira;

internal interface IKeyframeWriterContext
{
    public void AddBlendshape(Transform target, string name, float time, float value);
    public void AddBlendshapeDefault(Transform target, string name) => AddBlendshape(target, name, 0, 0);
    public void AddCancelBlendshape(Transform target, string name, float time, float value);
    public void AddCancelBlendshapeDefault(Transform target, string name) => AddCancelBlendshape(target, name, 0, 0);
    public void AddRotation(Transform target, float time, Vector3 eularAngle, bool relative = true);
    public void SetAvatarParameter<T>(string name, T value);
    public void SetAnimatorParameter<T>(string name, float time, T value);

    public void Flush() { }
}