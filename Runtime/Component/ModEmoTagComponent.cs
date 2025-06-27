using VRC.SDKBase;

namespace Numeira;

internal abstract class ModEmoTagComponent : MonoBehaviour, IEditorOnly, IModEmoComponent
{
    internal const string ComponentMenuPrefix = "ModEmo/ModEmo ";
}

internal interface IModEmoComponent 
{
    public Component? Component => this as Component;
    public GameObject? GameObject => Component?.gameObject;
}