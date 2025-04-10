using nadena.dev.modular_avatar.core;

namespace Numeira;

public readonly partial struct AnimatorParameter : IEquatable<AnimatorParameter>
{
    public readonly string Name;
    public readonly AnimatorParameterType Type;
    public readonly float Value;
    public readonly AnimatorParameterType? SyncType;
    public readonly bool Saved;
    public readonly bool IsLocal;


    public AnimatorParameter(string name, float value, AnimatorParameterType? syncType = null, bool saved = false, bool isLocal = true) : this(name, AnimatorParameterType.Float, value, syncType, saved, isLocal) { }
    public AnimatorParameter(string name, int value, AnimatorParameterType? syncType = null, bool saved = false, bool isLocal = true) : this(name, AnimatorParameterType.Int, value, syncType, saved, isLocal) { }
    public AnimatorParameter(string name, bool value, AnimatorParameterType? syncType = null, bool saved = false, bool isLocal = true) : this(name, AnimatorParameterType.Bool, value ? 1 : 0, syncType, saved, isLocal) { }

    public AnimatorParameter(string name, AnimatorParameterType type, float value, AnimatorParameterType? syncType, bool saved, bool isLocal)
    {
        Name = name;
        Type = type;
        Value = value;
        SyncType = syncType;
        Saved = saved;
        IsLocal = isLocal;
    }

    public bool Equals(AnimatorParameter other)
        => this.Name == other.Name &&
           this.Type == other.Type &&
           this.Value == other.Value;

    public override bool Equals(object obj) => obj is AnimatorParameter param && Equals(param);

    public override int GetHashCode() => HashCode.Combine(Name, Type, Value);

    public static explicit operator AnimatorControllerParameter(in AnimatorParameter parameter) => new()
    {
        name = parameter.Name,
        type = (AnimatorControllerParameterType)parameter.Type,
        defaultBool = parameter.Value != 0,
        defaultInt = (int)parameter.Value,
        defaultFloat = parameter.Value,
    };

    public static explicit operator ParameterConfig(in AnimatorParameter parameter) => new()
    {
        nameOrPrefix = parameter.Name,
        syncType = parameter.SyncType switch
        {
            null => ParameterSyncType.NotSynced,
            { } type => ConvertType(type),
        },
        localOnly = parameter.IsLocal,
        defaultValue = parameter.Value,
        saved = parameter.Saved,
    };

    private static ParameterSyncType ConvertType(AnimatorParameterType type) => type switch
    {
        AnimatorParameterType.Float => ParameterSyncType.Float,
        AnimatorParameterType.Int => ParameterSyncType.Int,
        AnimatorParameterType.Bool => ParameterSyncType.Bool,
        _ => ParameterSyncType.NotSynced,
    };
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