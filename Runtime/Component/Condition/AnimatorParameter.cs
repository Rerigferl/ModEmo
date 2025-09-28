
namespace Numeira
{
    [Serializable]
    internal struct AnimatorParameterCondition : IEquatable<AnimatorParameterCondition>
    {
        public AnimatorParameter Parameter;
        public ConditionMode Mode;

        public AnimatorParameterCondition(AnimatorParameter parameter, ConditionMode mode)
        {
            Parameter = parameter;
            Mode = mode;
        }

        public readonly bool Equals(AnimatorParameterCondition other)
            => Parameter.Equals(other.Parameter) && Mode == other.Mode;

        public readonly override int GetHashCode() => HashCode.Combine(Parameter, Mode);

        public readonly AnimatorParameterCondition Reverse()
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
    internal struct AnimatorParameter : IEquatable<AnimatorParameter>
    {
        public string Name;
        public AnimatorParameterValue Value;

        public AnimatorParameter(string name, AnimatorParameterValue value)
        {
            Name = name;
            Value = value;
        }

        public readonly bool Equals(AnimatorParameter other) => Name == other.Name && Value.Equals(other.Value);

        public readonly override int GetHashCode() => HashCode.Combine(Name.GetFarmHash64(), Value);

        public sealed class EqualityComparer : IEqualityComparer<AnimatorParameter>
        {
            public static EqualityComparer Default { get; } = new EqualityComparer();
            bool IEqualityComparer<AnimatorParameter>.Equals(AnimatorParameter x, AnimatorParameter y) => x.Equals(y);
            int IEqualityComparer<AnimatorParameter>.GetHashCode(AnimatorParameter obj) => obj.GetHashCode();
        }
    }

    [Serializable]
    internal struct AnimatorParameterValue : IEquatable<AnimatorParameterValue>
    {
        public AnimatorControllerParameterType Type;
        public float Value;

        public AnimatorParameterValue(float value, AnimatorControllerParameterType type)
        {
            Value = value;
            Type = type;
        }

        public AnimatorParameterValue(float value) : this(value, AnimatorControllerParameterType.Float) { }
        public AnimatorParameterValue(int value) : this(value, AnimatorControllerParameterType.Int) { }
        public AnimatorParameterValue(bool value) : this(value ? 1f : 0f, AnimatorControllerParameterType.Bool) { }

        public readonly override bool Equals(object obj)
            => obj is AnimatorParameter x && Equals(x);

        public readonly bool Equals(AnimatorParameterValue other)
            => this.Type == other.Type && this.Value == other.Value;

        public readonly override int GetHashCode() => HashCode.Combine(Type, Value);

        public static bool operator ==(AnimatorParameterValue left, AnimatorParameterValue right) => left.Equals(right);
        public static bool operator !=(AnimatorParameterValue left, AnimatorParameterValue right) => !left.Equals(right);

        public static implicit operator float(in AnimatorParameterValue value) => value.Value;
        public static explicit operator int(in AnimatorParameterValue value) => (int)value.Value;
        public static explicit operator bool(in AnimatorParameterValue value) => value.Value != 0;
        public static implicit operator AnimatorParameterValue(float value) => new(value);
        public static implicit operator AnimatorParameterValue(int value) => new(value);
        public static implicit operator AnimatorParameterValue(bool value) => new(value);

    }

#if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(AnimatorParameterCondition))]
    internal sealed class AnimatorParameterConditionDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var paramerterProp = property.FindPropertyRelative("Parameter");
            var modeProp = property.FindPropertyRelative("Mode");

            var nameProp = paramerterProp.FindPropertyRelative("Name");
            var valueProp = paramerterProp.FindPropertyRelative("Value");
            var (left, right) = new GUIPosition(position).HorizontalSeparate(margin: 2);

            var (left1, left2) = left.HorizontalSeparate(64, 2);

            EditorGUI.PropertyField(left1, valueProp.FindPropertyRelative("Type"), GUIContent.none);
            EditorGUI.PropertyField(left2, nameProp, GUIContent.none);

            var (right1, right2) = right.HorizontalSeparate(right.Width * 0.6f, 2);

            EditorGUI.PropertyField(right1, modeProp, GUIContent.none);
            EditorGUI.PropertyField(right2, valueProp, GUIContent.none);
        }
    }

    [CustomPropertyDrawer(typeof(AnimatorParameterValue))]
    internal sealed class AnimatorParameterValueDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var pos = (GUIPosition)position;
            var typeProp = property.FindPropertyRelative("Type");
            var type = Enums<AnimatorControllerParameterType>.Default[typeProp.enumValueIndex];

            var valueProp = property.FindPropertyRelative("Value");
            var value = valueProp.floatValue;
            EditorGUI.BeginChangeCheck();
            switch (type)
            {
                case AnimatorControllerParameterType.Float:
                    value = EditorGUI.FloatField(pos, GUIContent.none, value);
                    break;
                case AnimatorControllerParameterType.Int:
                    value = EditorGUI.IntField(pos, GUIContent.none, Mathf.FloorToInt(value));
                    break;
                case AnimatorControllerParameterType.Bool:
                case AnimatorControllerParameterType.Trigger:
                    value = EditorGUI.Toggle(pos.Center(EditorStyles.toggle.CalcSize(GUIContent.none)), GUIContent.none, value != 0) ? 1 : 0;
                    break;
                default:
                    EditorGUI.PropertyField(pos, valueProp, GUIContent.none);
                    break;
            }
            if (EditorGUI.EndChangeCheck())
            {
                valueProp.floatValue = value;
            }
        }
    }
#endif
}