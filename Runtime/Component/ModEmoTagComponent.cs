
using UnityEngine.Serialization;

namespace Numeira
{
    internal abstract class ModEmoTagComponent : MonoBehaviour, IModEmoComponent
    {
        internal const string ComponentMenuPrefix = "ModEmo/ModEmo ";

        protected abstract void CalculateContentHash(ref HashCode hashCode);

        void IModEmoComponent.CalculateContentHash(ref HashCode hashCode) => CalculateContentHash(ref hashCode);
    }

    internal abstract class ModEmoNamedComponent : ModEmoTagComponent, IModEmoNamedComponent
    {
        public string Name = "";

        string IModEmoNamedComponent.Name => string.IsNullOrEmpty(Name) ? name : Name;
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