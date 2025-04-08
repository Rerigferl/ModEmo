namespace Numeira;

public readonly partial struct AnimatorParameter : IEquatable<AnimatorParameter>
{
    public readonly string Name;
    public readonly AnimatorParameterType Type;
    public readonly float Value;

    public AnimatorParameter(string name, AnimatorParameterType type, float value)
    {
        Name = name;
        Type = type;
        Value = value;
    }

    public AnimatorParameter(string name, float value) : this(name, AnimatorParameterType.Float, value) { }
    public AnimatorParameter(string name, int value) : this(name, AnimatorParameterType.Int, value) { }
    public AnimatorParameter(string name, bool value) : this(name, AnimatorParameterType.Bool, value ? 1 : 0) { }

    public AnimatorControllerParameter ToUnityParameter() => new()
    {
        name = Name,
        type = (AnimatorControllerParameterType)Type,
        defaultBool = Value != 0,
        defaultInt = (int)Value,
        defaultFloat = Value,
    };

    public bool Equals(AnimatorParameter other)
        => this.Name == other.Name &&
           this.Type == other.Type &&
           this.Value == other.Value;

    public override bool Equals(object obj) => obj is AnimatorParameter param && Equals(param);

    public override int GetHashCode() => HashCode.Combine(Name, Type, Value);
}

public enum AnimatorParameterType
{
    Float = 1,
    Int = 3,
    Bool = 4,
}

partial struct AnimatorParameter
{
    public sealed class ParameterNameEqualityComparer : IEqualityComparer<AnimatorParameter>
    {
        public static ParameterNameEqualityComparer Instance { get; } = new();

        public bool Equals(AnimatorParameter x, AnimatorParameter y) => x.Name == y.Name;

        public int GetHashCode(AnimatorParameter obj) => obj.Name.GetHashCode();
    }
}