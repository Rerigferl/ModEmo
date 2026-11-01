namespace Numeira;

internal interface ISubComponent : IModEmoComponent
{

}

internal interface ISubComponent<T> : ISubComponent where T : ISubComponent<T>
{
    private static readonly List<IOwnerComponent<T>> temporaryList = new();

    public IOwnerComponent<T>? GetOwner()
    {
        if (this is IOwnerComponent<T>)
        {
            var components = GameObject.GetCachedComponents();

            if (components.Count != 0)
            {
                foreach (var component in components)
                {
                    if (component is not IOwnerComponent<T> o || component is not ISubComponent<T> s)
                        continue;

                    if (o == this)
                        continue;

                    return o;
                }
            }
        }
        else
        {
            if (GameObject.TryGetCachedComponent<IOwnerComponent<T>>(out var self))
                return self;
        }

        var parent = GameObject.transform.parent;
        if (parent != null && parent.gameObject.TryGetCachedComponent<IOwnerComponent<T>>(out var p))
            return p;

        return null;
    }
}