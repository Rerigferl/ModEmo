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
            var components = temporaryList;
            GameObject.GetComponents(components);

            if (components.Count != 0)
            {
                foreach (var component in components)
                {
                    if (component == this)
                        continue;

                    return component;
                }
            }
        }
        else
        {
            if (GameObject.TryGetComponent<IOwnerComponent<T>>(out var self))
                return self;
        }

        var parent = GameObject.transform.parent;
        if (parent != null && parent.TryGetComponent<IOwnerComponent<T>>(out var p))
            return p;

        return null;
    }
}