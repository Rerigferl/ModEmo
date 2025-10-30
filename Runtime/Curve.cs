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

    public int Length => keys?.Count ?? 0;

    public IEnumerable<Keyframe> Keys => keys;

    public ref Keyframe this[int index] => ref keys.AsSpan()[index];

    public float Evaluate(float time)
    {
        if (Length == 0)
            return 0;

        if (keys.Count == 1 && keys[0].Time > 0)
        {
            keys.Add(default);
        }
        keys!.Sort((x, y) => x.Time.CompareTo(y.Time));

        return Evaluate(keys.AsSpan(), time);
    }

    public void AddKey(float time, float value) => AddKey(new(time, value));

    public void AddKey(Keyframe keyframe) => keys.Add(keyframe);

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
        foreach(var key in curve.keys)
        {
            result.AddKey(key.time, key.value);
        }
        return result;
    }

    public struct Keyframe
    {
        public float Time;
        public float Value;

        public Keyframe(float time, float value)
        {
            Time = time;
            Value = value;
        }
    }
}
