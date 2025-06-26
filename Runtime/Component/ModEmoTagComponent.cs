using VRC.SDKBase;

namespace Numeira;

internal abstract class ModEmoTagComponent : MonoBehaviour, IEditorOnly, IModEmoComponent
{
    internal const string ComponentMenuPrefix = "ModEmo/ModEmo ";
}

internal interface IModEmoComponent { }