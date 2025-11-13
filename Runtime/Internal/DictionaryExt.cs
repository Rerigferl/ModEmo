using System;
using UnityEngine.UIElements;

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

    public static bool TryGetValue<TValue>(this Dictionary<string, TValue> dictionary, ReadOnlySpan<char> key, out TValue? result)
    {
        var accessor = Unsafe.As<DictionaryAccessor<string, TValue>>(dictionary);

        var index = FindEntry(accessor, key);
        if (index == -1)
            goto Fail;
        result = accessor.entries.AsSpan()[index].Value;
        return true;

        static int FindEntry(DictionaryAccessor<string, TValue> dictionary, ReadOnlySpan<char> key)
        {
            var buckets = dictionary.buckets!;
            var entries = dictionary.entries!;
            var comparer = dictionary.comparer!;
            if (buckets != null)
            {
                int num = GetHashCode(key) & 0x7FFFFFFF;
                for (int num2 = buckets[num % buckets.Length]; num2 >= 0; num2 = entries[num2].Next)
                {
                    if (entries[num2].HashCode == num && key.Equals(entries[num2].Key, StringComparison.Ordinal))
                    {
                        return num2;
                    }
                }
            }

            return -1;
        }

        static unsafe int GetHashCode(ReadOnlySpan<char> span)
        {
            fixed (char* ptr = span)
            {
                int num = 352654597;
                int num2 = num;
                int* ptr2 = (int*)ptr;
                int num3;
                for (num3 = span.Length; num3 > 2; num3 -= 4)
                {
                    num = ((num << 5) + num + (num >> 27)) ^ *ptr2;
                    num2 = ((num2 << 5) + num2 + (num2 >> 27)) ^ ptr2[1];
                    ptr2 += 2;
                }

                if (num3 > 0)
                {
                    num = ((num << 5) + num + (num >> 27)) ^ *ptr2;
                }

                return num + num2 * 1566083941;
            }
        }

    Fail:
        result = default;
        return false;
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
