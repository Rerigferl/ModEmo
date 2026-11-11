namespace Numeira;

internal interface IOwnerComponent : IModEmoComponent
{
    public bool AllowSelf => true;
}

internal interface IOwnerComponent<T> : IOwnerComponent where T : ISubComponent<T>
{

}

internal static class OwnerCompoenentExt
{
    public static IEnumerable<T> GetOwnedComponents<T>(this IOwnerComponent<T> owner) where T : ISubComponent<T>
    {
        return GetOwnedComponents<IOwnerComponent<T>, T>(owner); 
    }

    public static IEnumerable<TSub> GetOwnedComponents<TOwner, TSub>(this TOwner owner) where TOwner : IOwnerComponent<TSub> where TSub : ISubComponent<TSub>
    {
        if (!owner.GameObject.activeInHierarchy)
            yield break;

        static IEnumerable<TSub> GetOwnedComponentsInternal(GameObject go, GameObject? owner = null)
        {
            owner ??= go;
            var x = typeof(TSub).FullName;
            foreach (var c in go.GetCachedComponents())
            {
                if (c is not TSub sub)
                    continue;

                if (sub.GetOwner() is not { } subOwner)
                    continue;

                if (subOwner.GameObject == owner)
                    yield return sub;
            }
        }

        if (owner.AllowSelf)
        {
            foreach (var x in GetOwnedComponentsInternal(owner.GameObject))
                yield return x;
        }

        foreach(var child in owner.GameObject.GetCachedChildren())
        {
            foreach (var x in GetOwnedComponentsInternal(child, owner.GameObject))
                yield return x;
        }
    }

    public static IEnumerable<T> GetOwnedComponents<T>(this IOwnerComponent owner) where T : ISubComponent<T>
        => (owner is IOwnerComponent<T> o) ? o.GetOwnedComponents() : owner.GetOwnedComponents<T>();

}