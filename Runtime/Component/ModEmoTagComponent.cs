namespace Numeira
{
    internal abstract class ModEmoTagComponent : MonoBehaviour, IModEmoComponent
    {
        internal const string ComponentMenuPrefix = "ModEmo/ModEmo ";

        protected abstract void CalculateContentHash(ref HashCode hashCode);

        void IModEmoComponent.CalculateContentHash(ref HashCode hashCode) => CalculateContentHash(ref hashCode);
    }

    internal abstract class ModEmoNamedTagComponent : ModEmoTagComponent, IModEmoNamedComponent
    {
        public string Name = "";

        protected virtual string GetName() => Name;

        string IModEmoNamedComponent.Name
        {
            get
            {
                var name = GetName();
                return string.IsNullOrEmpty(name) ? this.name : name;
            }
        }
    }

    internal interface IModEmoNamedComponent : IModEmoComponent
    {
        string Name { get; }
    }

    internal interface IModEmoComponent : INDMFEditorOnly
    {
        public Component Component => (this as Component)!;
        public GameObject GameObject => Component.gameObject;

        public void CalculateContentHash(ref HashCode hashCode);
    }
}