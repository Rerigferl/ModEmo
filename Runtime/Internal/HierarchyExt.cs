namespace Numeira;

internal static class HierarchyExt
{
    public struct ChildrenEnumerator
    {
        private readonly Transform parent;
        private int index;

        public readonly GameObject Current => parent.GetChild(index).gameObject;

        public ChildrenEnumerator(Transform parent)
        {
            this.parent = parent;
            index = -1;
        }

        public bool MoveNext()
        {
            int childCount = parent.childCount;
            return ++index < childCount;
        }

        public void Reset()
        {
            index = -1;
        }
    }

    public static ChildrenEnumerator GetEnumerator(this GameObject go)
    {
        return new ChildrenEnumerator(go.transform);
    }
}
