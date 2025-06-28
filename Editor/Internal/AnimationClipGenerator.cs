using UnityKeyframe = UnityEngine.Keyframe;

namespace Numeira;

internal sealed class AnimationClipGenerator
{
    public string Name { get; set; } = "";
    private readonly Dictionary<EditorCurveBinding, HashSet<Keyframe>> bindings = new();

    public void Add(EditorCurveBinding key, float time, float value)
    {
        var hashSet = bindings.GetOrAdd(key, _ => new(Keyframe.TimeEqualityComparer.Defualt));
        var item = new Keyframe(time, value);
        if (hashSet.Contains(item))
        {
            hashSet.Remove(item);
        }
        hashSet.Add(item);
    }

    public AnimationClip Export()
    {
        AnimationClip result = new() { name = Name };
        var keys = bindings.Keys;
        var times = new SortedSet<float>(bindings.Values.SelectMany(x => x).Select(x => x.Time)).ToArray();
        
        var dict = bindings.ToDictionary(x => x.Key, x => x.Value.ToDictionary(x => x.Time, x => x));

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
                if (dict[binding].TryGetValue(times[i2], out var value))
                    return value;

                // search previous...
                while (i2 != 0)
                {
                    if (dict[binding].TryGetValue(times[--i2], out value))
                        return value;
                }

                // search next...
                i2 = (uint)i;
                while (i2 < times.Length - 1)
                {
                    if (dict[binding].TryGetValue(times[++i2], out value))
                        return value;
                }

                return default;
            }
        }

        foreach(var (key, value) in curves)
        {
            var array = value.ToArray();
            for(int i = 0; i < array.Length; i++)
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
            AnimationUtility.SetEditorCurve(result, key, new(array));
        }

        return result;
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
            public static IEqualityComparer<Keyframe> Defualt { get; } = new TimeEqualityComparer();

            bool IEqualityComparer<Keyframe>.Equals(Keyframe x, Keyframe y) => x.Time == y.Time;
            int IEqualityComparer<Keyframe>.GetHashCode(Keyframe obj) => obj.Time.GetHashCode();
        }
    }



}
