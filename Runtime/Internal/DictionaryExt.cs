using System.Runtime.Serialization;

namespace Numeira;

internal static class DictionaryExt
{
    public static TValue GetOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory)
    {
        if (dictionary.TryGetValue(key, out TValue value))
            return value;
        value = factory(key);
        dictionary.Add(key, value);
        return value;
    }

    public static ref TValue GetValueRefOrNullRef<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
    {
        var accessor = Unsafe.As<DictionaryAccessor<TKey, TValue>>(dictionary);

        var index = FindEntry(accessor, key);
        if (index == -1)
            return ref Unsafe.NullRef<TValue>();
        return ref accessor.entries.AsSpan()[index].Value;

        static int FindEntry(DictionaryAccessor<TKey, TValue> dictionary, TKey key)
        {
            var buckets = dictionary.buckets!;
            var entries = dictionary.entries!;
            var comparer = dictionary.comparer!;
            if (buckets != null)
            {
                int num = comparer.GetHashCode(key) & 0x7FFFFFFF;
                for (int num2 = buckets[num % buckets.Length]; num2 >= 0; num2 = entries[num2].Next)
                {
                    if (entries[num2].HashCode == num && comparer.Equals(entries[num2].Key, key))
                    {
                        return num2;
                    }
                }
            }

            return -1;
        }
    }

    public static ref TValue GetValueRefOrAdd<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, Func<TKey, TValue> factory)
    {
        ref var x = ref dictionary.GetValueRefOrNullRef(key);
        if (Unsafe.IsNullRef(ref x))
        {
            dictionary.Add(key, factory(key));
            x = ref dictionary.GetValueRefOrNullRef(key)!;
        }
        return ref x!;
    }

    public static Span<DictionaryEntry<TKey, TValue>> GetEntries<TKey, TValue>(this Dictionary<TKey, TValue> dictionary)
    {
        var accessor = Unsafe.As<DictionaryAccessor<TKey, TValue>>(dictionary);

        return accessor.entries.AsSpan(0, accessor.count);
    }

    public struct DictionaryEntry<TKey, TValue>
    {
        public int HashCode;
        public int Next;
        public TKey Key;
        public TValue Value;
    }

    private class DictionaryAccessor<TKey, TValue>
    {
        public int[]? buckets;

        public DictionaryEntry<TKey, TValue>[]? entries;

        public int count;

        private int version;

        private int freeList;

        private int freeCount;

        public IEqualityComparer<TKey>? comparer;

        private object? keys;

        private object? values;

        private object? _syncRoot;

    }
}
