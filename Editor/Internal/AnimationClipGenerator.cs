using System.Buffers;
using Numeira.Animation;
using UnityKeyframe = UnityEngine.Keyframe;

namespace Numeira;

internal sealed class AnimationClipGenerator
{
    public string Name { get; set; } = "";
    private readonly Dictionary<EditorCurveBinding, List<Keyframe>> keyframeMap = new();

    public void Add(EditorCurveBinding key, float time, float value)
    {
        var list = keyframeMap.GetOrAdd(key, _ => new());
        if (list.Count > 0)
        {
            foreach (var x in list.AsSpan())
            {
                if (Mathf.Approximately(x.Time, time))
                    return;
            }
        }

        var item = new Keyframe(time, value);
        if (list.Count == 0)
        {
            list.Add(item);
            return;
        }

        var span = list.AsSpan();
        if (span[0].Time > time)
        {
            list.Insert(0, item);
            return;
        }
        if (span[^1].Time < time)
        {
            list.Add(item);
            return;
        }

        int left = 0, right = span.Length - 1;
        while (left < right)
        {
            int mid = (left + right) / 2;
            if (span[mid].Time < time)
                left = mid + 1;
            else
                right = mid;
        }
        list.Insert(left, item);
    }

    public void Clear()
    {
        keyframeMap.Clear();
    }

    private static float Evaluate(ReadOnlySpan<Keyframe> sortedKeyframes, float time)
    {
        if (sortedKeyframes.IsEmpty)
            return 0;

        if (sortedKeyframes.Length == 1)
            return sortedKeyframes[0].Value;

        if (sortedKeyframes[0].Time < time)
            return sortedKeyframes[0].Value;

        if (sortedKeyframes[^1].Time > time)
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

    public AnimationClip Export()
    {
        AnimationClip result = new() { name = Name };
        var keys = keyframeMap.Keys;
        var times = keyframeMap.Values.SelectMany(x => x).Select(x => x.Time).Distinct().OrderBy(x => x).ToArray();

        var dict = keyframeMap.ToDictionary(x => x.Key, x => x.Value.ToDictionary(x => x.Time, x => x));

        var curves = new Dictionary<EditorCurveBinding, List<UnityKeyframe>>();

        for (int i = 0; i < times.Length; i++)
        {
            float time = times[i];

            foreach (var key in keys)
            {
                curves.GetOrAdd(key, _ => new()).Add(FindKeyframe(key).ToUnityKeyframe());
            }

            Keyframe FindKeyframe(EditorCurveBinding binding)
            {
                uint i2 = (uint)i;
                var list = dict[binding];
                if (list.TryGetValue(times[i2], out var value))
                    return value;

                // search previous...
                while (i2 > 0)
                {
                    if (list.TryGetValue(times[--i2], out value))
                        return value;
                }

                // search next...
                i2 = (uint)i;
                while (i2 < times.Length - 1)
                {
                    if (list.TryGetValue(times[++i2], out value))
                        return value;
                }

                return default;
            }
        }

        var curve = new AnimationCurve();
        foreach (var (key, value) in curves)
        {
            var array = value.ToArray();
            for (int i = 0; i < array.Length; i++)
            {
                if (i != 0)
                {
                    array[i].inTangent = Tangent(array[i - 1].time, array[i].time, array[i - 1].value, array[i].value);
                }
                if (i < array.Length - 1)
                {
                    array[i].outTangent = Tangent(array[i].time, array[i + 1].time, array[i].value, array[i + 1].value);
                }
            }
            curve.keys = array;
            AnimationUtility.SetEditorCurve(result, key, curve);
        }

        return result;
    }


    public AnimationClip Export2()
    {
        AnimationClip result = new() { name = Name };
        HashSet<float> timesSet = new(FloatEqualityComparer.Default);
        var map = keyframeMap;

        foreach (var (binding, keyframes) in map)
        {
            foreach (var keyframe in keyframes)
            {
                timesSet.Add(keyframe.Time);
            }
        }

        var times = timesSet.ToArray();
        var keys = new UnityKeyframe[times.Length];
        var curve = new AnimationCurve(keys);

        foreach (var (binding, keyframes) in map)
        {
            for (int i = 0; i < keys.Length; i++)
            {
                keys[i] = new UnityKeyframe(times[i], Evaluate(keyframes.AsSpan(), times[i]));
            }
            curve.keys = keys;
            result.SetEditorCurveNoSync(binding, curve);
        }

        result.SyncEditorCurves();

        return result;
    }

    private sealed class FloatEqualityComparer : IEqualityComparer<float>
    {
        public static FloatEqualityComparer Default { get; } = new();
        public bool Equals(float x, float y) => Mathf.Approximately(x, y);

        public int GetHashCode(float obj) => obj.GetHashCode();
    }

    private static float Tangent(float timeStart, float timeEnd, float valueStart, float valueEnd)
    {
        return (valueEnd - valueStart) / (timeEnd - timeStart);
    }

    private struct Keyframe
    {
        public float Time;
        public float Value;

        public Keyframe(float time, float value)
        {
            Time = time;
            Value = value;
        }

        public readonly UnityKeyframe ToUnityKeyframe() => new(Time, Value);

        public sealed class TimeEqualityComparer : IEqualityComparer<Keyframe>
        {
            public static TimeEqualityComparer Defualt { get; } = new();

            bool IEqualityComparer<Keyframe>.Equals(Keyframe x, Keyframe y) => x.Time == y.Time;
            int IEqualityComparer<Keyframe>.GetHashCode(Keyframe obj) => obj.Time.GetHashCode();
        }
    }

    private sealed class Keyframes
    {
        private Keyframe[]? items;
        private int count = 0;

        public Keyframes()
        {
            items = ArrayPool<Keyframe>.Shared.Rent(2);
        }

        public void Add(Keyframe value)
        {
            var span = items is null ? Span<Keyframe>.Empty : items.AsSpan();
            if (span.IsEmpty)
                return;

            var count = this.count;
            if (span.Length >= count + 1)
            {
                var newArray = ArrayPool<Keyframe>.Shared.Rent(span.Length + 1);
                span.CopyTo(newArray);
                ArrayPool<Keyframe>.Shared.Return(newArray);
                span = items = newArray;
            }

            if (count == 0)
            {
                span[0] = value;
                this.count += 1;
                return;
            }

            var time = value.Time;
            if (span[0].Time > time)
            {
                Insert(span, count, 0, value);
                this.count += 1;
                return;
            }
            if (span[^1].Time < time)
            {
                span[count++] = value;
                this.count += 1;
                return;
            }

            int left = 0, right = count - 1;
            while (left < right)
            {
                int mid = (left + right) / 2;
                if (span[mid].Time < time)
                    left = mid + 1;
                else
                    right = mid;
            }
            Insert(span, count, left, value);
            this.count += 1;

            static void Insert(Span<Keyframe> span, int count, int index, Keyframe item)
            {
                for (int i = count - 1; i >= index; i--)
                {
                    span[i + 1] = span[i];
                }

                span[index] = item;
            }
        }

        public int Count => count;
        public ReadOnlySpan<Keyframe> Span => items is null ? default : items.AsSpan(0, count);

        public void Dispose()
        {
            if (items is null)
                return;
            ArrayPool<Keyframe>.Shared.Return(items);
            items = null;
        }
    }
}



internal sealed class AnimationClipBuilderWriter : AnimationWriter
{
    public AnimationClipBuilder Builder { get; }

    public AnimationClipBuilderWriter(AnimationClipBuilder builder)
    {
        Builder = builder;
    }

    protected override void Write(AnimationBinding binding, Curve.Keyframe keyframe)
    {
        Builder.Add(binding, keyframe.Time, keyframe.Value);
    }
}