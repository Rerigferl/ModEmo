namespace Numeira;

internal interface IOwnerComponent : IModEmoComponent
{
}

internal interface IOwnerComponent<T> : IOwnerComponent where T : ISubComponent<T>
{
    public IEnumerable<T> GetOwnedComponents()
    {
        foreach (var x in Component.GetComponentsInDirectChildren<T>(includeSelf: true))
        {
            if (x.GetOwner() == this)
                yield return x;
        }
    }
}

internal static class OwnerCompoenentExt
{
    public static IEnumerable<T> GetOwnedComponents<T>(this IOwnerComponent<T> owner) where T : ISubComponent<T>
        => owner.GetOwnedComponents();

    public static IEnumerable<T> GetOwnedComponents<T>(this IOwnerComponent owner) where T : ISubComponent<T>
        => (owner is IOwnerComponent<T> o) ? o.GetOwnedComponents() : owner.GetOwnedComponentsDirect<T>();

    private static IEnumerable<T> GetOwnedComponentsDirect<T>(this IOwnerComponent owner) where T : ISubComponent<T>
    {
        foreach (var x in owner.Component.GetComponentsInDirectChildren<T>(includeSelf: true))
        {
            if (x.GetOwner() == owner)
                yield return x;
        }
    }
}