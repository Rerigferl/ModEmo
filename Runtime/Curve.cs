using System.Runtime.InteropServices;

namespace Numeira;

[Serializable]
[StructLayout(LayoutKind.Sequential)]
internal sealed class Curve
{
    [SerializeField]
    private List<Keyframe> keys;

    public Curve()
    {
        keys = new();
    }

    public Curve(Keyframe key)
    {
        keys = new() { key };
    }

    public Curve(Keyframe key1, Keyframe key2)
    {
        keys = new() { key1, key2 };
    }

    public Curve(params Keyframe[] keys)
    {
        this.keys = new(keys);
    }

    public Curve(IEnumerable<Keyframe> keys)
    {
        this.keys = new(keys);
    }

    internal void Reset() => keys.Clear();

    public int Length => keys?.Count ?? 0;

    public IEnumerable<Keyframe> Keys => keys;

    public ref Keyframe this[int index] => ref keys.AsSpan()[index];

    public float Evaluate(float time)
    {
        if (Length == 0)
            return 0;

        if (keys.Count == 1 && keys[0].Time > 0)
        {
            AddKey(0, 0);
        }

        return Evaluate(keys.AsSpan(), time);
    }

    public void AddKey(float time, float value) => AddKey(new(time, value));

    public void AddKey(Keyframe keyframe, bool update = true)
    {
        int index = keys.AsSpan().BinarySearch(keyframe, TimeEqualityComparer.Default);
        if (index >= 0)
        {
            if (update)
            {
                keys[index] = keyframe;
            }

            return;
        }

        keys.Insert(~index, keyframe);
    }

    private static float Evaluate(ReadOnlySpan<Keyframe> sortedKeyframes, float time)
    {
        if (sortedKeyframes.IsEmpty)
            return 0;

        if (sortedKeyframes.Length == 1)
            return sortedKeyframes[0].Value;

        if (sortedKeyframes[0].Time > time)
            return sortedKeyframes[0].Value;

        if (sortedKeyframes[^1].Time < time)
            return sortedKeyframes[^1].Value;

        for (int i = 0; i < sortedKeyframes.Length - 1; i++)
        {
            if (time >= sortedKeyframes[i].Time && time <= sortedKeyframes[i + 1].Time)
            {
                Keyframe leftKey = sortedKeyframes[i];
                Keyframe rightKey = sortedKeyframes[i + 1];

                float t = (time - leftKey.Time) / (rightKey.Time - leftKey.Time);
                return leftKey.Value + (rightKey.Value - leftKey.Value) * t;
            }
        }

        return 0;
    }

    public static explicit operator Curve(AnimationCurve curve)
    {
        var result = new Curve();
        foreach (var key in curve.keys)
        {
            result.AddKey(key.time, key.value);
        }
        return result;
    }

    [Serializable]
    public struct Keyframe : IEquatable<Keyframe>
    {
        public float Time;
        public float Value;

        public InOutPair<float> Tangent;

        public Keyframe(float time, float value)
        {
            Time = time;
            Value = value;
            Tangent = default;
        }

        public Keyframe(float time, float value, InOutPair<float> tangent)
        {
            Time = time;
            Value = value;
            Tangent = tangent;
        }

        public readonly override bool Equals(object obj) => obj is Keyframe other && Equals(other);

        public readonly bool Equals(in Keyframe other) => Time == other.Time && Value == other.Value && Tangent == other.Tangent;

        readonly bool IEquatable<Keyframe>.Equals(Keyframe other) => Equals(other);

        public readonly override int GetHashCode() => HashCode.Combine(Time, Value, Tangent);

        public static bool operator ==(in Keyframe left, in Keyframe right) => left.Equals(right);
        public static bool operator !=(in Keyframe left, in Keyframe right) => left.Equals(right);
    }

    public sealed class TimeEqualityComparer : IEqualityComparer<Keyframe>, IComparer<Keyframe>
    {
        public static TimeEqualityComparer Default { get; } = new();

        int IComparer<Keyframe>.Compare(Keyframe x, Keyframe y) => x.Time.CompareTo(y.Time);

        bool IEqualityComparer<Keyframe>.Equals(Keyframe x, Keyframe y) => x.Time.Equals(y.Time);

        int IEqualityComparer<Keyframe>.GetHashCode(Keyframe obj) => obj.Time.GetHashCode();
    }
}

[Serializable]
internal struct InOutPair<T> : IEquatable<InOutPair<T>>
{
    public T In;
    public T Out;

    public InOutPair(T @in, T @out)
    {
        In = @in;
        Out = @out;
    }

    public static implicit operator InOutPair<T>((T x, T y) tuple) => new(tuple.x, tuple.y);

    public readonly void Deconstruct(out T @in, out T @out) => (@in, @out) = (In, Out);

    public readonly bool Equals(in InOutPair<T> other)
    {
        if (typeof(T).IsValueType)
        {
            return EqualityComparer<T>.Default.Equals(In, other.In) && EqualityComparer<T>.Default.Equals(Out, other.Out);
        }
        else
        {
            var comparer = EqualityComparer<T>.Default;
            return comparer.Equals(In, other.In) && comparer.Equals(Out, other.Out);
        }
    }

    readonly bool IEquatable<InOutPair<T>>.Equals(InOutPair<T> other) => Equals(other);

    public readonly override bool Equals(object obj) => obj is InOutPair<T> other && Equals(other);

    public readonly override int GetHashCode() => HashCode.Combine(In, Out);

    public static bool operator ==(in InOutPair<T> left, in InOutPair<T> right) => left.Equals(right);
    public static bool operator !=(in InOutPair<T> left, in InOutPair<T> right) => !left.Equals(right);
} 