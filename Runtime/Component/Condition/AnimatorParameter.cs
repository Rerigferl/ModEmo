namespace Numeira
{
    [Serializable]
    internal readonly struct AnimatorParameterCondition : IEquatable<AnimatorParameterCondition>
    {
        public readonly AnimatorParameter Parameter;
        public readonly ConditionMode Mode;

        public AnimatorParameterCondition(AnimatorParameter parameter, ConditionMode mode)
        {
            Parameter = parameter;
            Mode = mode;
        }

        public bool Equals(AnimatorParameterCondition other)
            => Parameter.Equals(other.Parameter) && Mode == other.Mode;

        public override int GetHashCode() => HashCode.Combine(Parameter, Mode);

        public AnimatorParameterCondition Reverse()
        {
            if (Mode == ConditionMode.Equals)
                return new(Parameter, ConditionMode.NotEqual);
            if (Mode == ConditionMode.NotEqual)
                return new(Parameter, ConditionMode.Equals);
            if (Mode == ConditionMode.GreaterThan)
                return new(Parameter, ConditionMode.LessThanOrEqual);
            if (Mode == ConditionMode.LessThan)
                return new(Parameter, ConditionMode.GreaterThanOrEqual);
            if (Mode == ConditionMode.GreaterThanOrEqual)
                return new(Parameter, ConditionMode.LessThan);
            if (Mode == ConditionMode.LessThanOrEqual)
                return new(Parameter, ConditionMode.GreaterThan);

            return this;
        }

        public sealed class EqualityComparer : IEqualityComparer<AnimatorParameterCondition>
        {
            public static EqualityComparer Default { get; } = new();

            bool IEqualityComparer<AnimatorParameterCondition>.Equals(AnimatorParameterCondition x, AnimatorParameterCondition y) => x.Equals(y);

            int IEqualityComparer<AnimatorParameterCondition>.GetHashCode(AnimatorParameterCondition obj) => obj.GetHashCode();
        }
    }

    public enum ConditionMode
    {
        None = 0,
        Equals = 1,
        NotEqual = 2,
        GreaterThan = 4,
        LessThan = 8,

        GreaterThanOrEqual = GreaterThan | Equals,
        LessThanOrEqual = LessThan | Equals,
    }

    [Serializable]
    internal readonly struct AnimatorParameter : IEquatable<AnimatorParameter>
    {
        public readonly string Name;
        public readonly AnimatorParameterValue Value;

        public AnimatorParameter(string name, AnimatorParameterValue value)
        {
            Name = name;
            Value = value;
        }

        public bool Equals(AnimatorParameter other) => Name == other.Name && Value.Equals(other.Value);

        public override int GetHashCode() => HashCode.Combine(Name, Value);

        public sealed class EqualityComparer : IEqualityComparer<AnimatorParameter>
        {
            public static EqualityComparer Default { get; } = new EqualityComparer();
            bool IEqualityComparer<AnimatorParameter>.Equals(AnimatorParameter x, AnimatorParameter y) => x.Equals(y);
            int IEqualityComparer<AnimatorParameter>.GetHashCode(AnimatorParameter obj) => obj.GetHashCode();
        }
    }

    [Serializable]
    internal readonly struct AnimatorParameterValue : IEquatable<AnimatorParameterValue>
    {
        public readonly AnimatorControllerParameterType Type;
        public readonly float Value;

        public AnimatorParameterValue(float value, AnimatorControllerParameterType type)
        {
            Value = value;
            Type = type;
        }

        public AnimatorParameterValue(float value) : this(value, AnimatorControllerParameterType.Float) { }
        public AnimatorParameterValue(int value) : this(value, AnimatorControllerParameterType.Int) { }
        public AnimatorParameterValue(bool value) : this(value ? 1f : 0f, AnimatorControllerParameterType.Bool) { }

        public override bool Equals(object obj)
            => obj is AnimatorParameter x && Equals(x);

        public bool Equals(AnimatorParameterValue other)
            => this.Type == other.Type && this.Value == other.Value;

        public override int GetHashCode() => HashCode.Combine(Type, Value);

        public static bool operator ==(AnimatorParameterValue left, AnimatorParameterValue right) => left.Equals(right);
        public static bool operator !=(AnimatorParameterValue left, AnimatorParameterValue right) => !left.Equals(right);

        public static implicit operator float(in AnimatorParameterValue value) => value.Value;
        public static explicit operator int(in AnimatorParameterValue value) => (int)value.Value;
        public static explicit operator bool(in AnimatorParameterValue value) => value.Value != 0;
        public static implicit operator AnimatorParameterValue(float value) => new(value);
        public static implicit operator AnimatorParameterValue(int value) => new(value);
        public static implicit operator AnimatorParameterValue(bool value) => new(value);

    }
}