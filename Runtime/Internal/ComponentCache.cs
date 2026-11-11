using System.Diagnostics.CodeAnalysis;

namespace Numeira;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
internal static class ComponentCache
{
    private static uint HIERARCHY_CHANGED_SEQ = uint.MinValue;
    private static readonly Dictionary<GameObject, GameObjectDataCache> cache = new();

    #if UNITY_EDITOR
    static ComponentCache()
    {
        EditorApplication.hierarchyChanged += () => HIERARCHY_CHANGED_SEQ = unchecked(HIERARCHY_CHANGED_SEQ + 1);
    }
    #endif

    public static List<Component> GetCachedComponents(this GameObject gameObject) 
        => cache.GetOrAdd(gameObject, static obj => new(obj)).Components;

    public static List<GameObject> GetCachedChildren(this GameObject gameObject) 
        => cache.GetOrAdd(gameObject, static obj => new(obj)).Children;

    public static bool TryGetCachedComponent<T>(this GameObject gameObject, [NotNullWhen(true)] out T? component)
    {
        var components = gameObject.GetCachedComponents();
        foreach(var x in components.AsSpan())
        {
            if (x is T y)
            {
                component = y;
                return true;
            }
        }
        component = default;
        return false;
    }

    public static T? GetCachedComponent<T>(this GameObject gameObject)
    {
        foreach(var component in GetCachedComponents(gameObject).AsSpan())
        {
            if (component is T result)
                return result;
        }
        return default;
    }

    private sealed class GameObjectDataCache
    {
        public GameObject GameObject { get; }
        private uint cachedSequence = uint.MaxValue;
        private readonly List<Component> components = new();
        private readonly List<GameObject> children = new();

        public List<Component> Components
        {
            get
            {
                if (cachedSequence != HIERARCHY_CHANGED_SEQ)
                    Refresh();
                return components;
            }
        }

        public List<GameObject> Children
        {
            get
            {
                if (cachedSequence != HIERARCHY_CHANGED_SEQ)
                    Refresh();
                return children;
            }
        }

        public GameObjectDataCache(GameObject gameObject)
        {
            GameObject = gameObject;
            Refresh();
        }

        public void Refresh()
        {
            cachedSequence = HIERARCHY_CHANGED_SEQ;

            GameObject.GetComponents(components);

            var tr = GameObject.transform;
            int count = tr.childCount;
            var children = this.children;
            children.Clear();
            children.Capacity = Math.Max(children.Capacity, count);
            for (int i = 0; i < count; i++)
            {
                children.Add(tr.GetChild(i).gameObject);
            }
        }
    }
}