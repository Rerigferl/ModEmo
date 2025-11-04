namespace Numeira;

internal sealed class Group
{
    public static IGrouping<TKey, TValue> Create<TKey, TValue>(TKey key, Func<TKey, IEnumerable<TValue>> factory)
        => new EnumerableGroup<TKey, TValue>(key, factory(key));
    public static IGrouping<TKey, TValue> Create<TKey, TValue>(TKey key, IEnumerable<TValue> values)
        => new EnumerableGroup<TKey, TValue>(key, values);

    internal sealed class EnumerableGroup<TKey, TValue> : IGrouping<TKey, TValue>
    {
        public EnumerableGroup(TKey key, IEnumerable<TValue> values)
        {
            Key = key;
            Values = values;
        }

        public TKey Key { get; }
        public IEnumerable<TValue> Values { get; }

        public IEnumerator<TValue> GetEnumerator() => Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}