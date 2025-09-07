namespace Numeira;

internal abstract class ModEmoTagComponent : MonoBehaviour, IModEmoComponent
{
    internal const string ComponentMenuPrefix = "ModEmo/ModEmo ";
}

internal interface IModEmoComponent : INDMFEditorOnly
{
    public Component Component => (this as Component)!;
    public GameObject GameObject => Component.gameObject;
}
