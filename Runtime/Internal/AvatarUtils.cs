using nadena.dev.ndmf.runtime;

namespace Numeira;

internal static class AvatarUtils
{
    public static SkinnedMeshRenderer? GetFaceRenderer(this ModEmo component)
        => GetFaceRenderer(component.transform);

    public static SkinnedMeshRenderer? GetFaceRenderer(Transform transform)
    {
        var avatarRoot = RuntimeUtil.FindAvatarInParents(transform);
        if (avatarRoot == null)
            return null;

#if VRC_SDK_VRCSDK3
        var descriptor = avatarRoot.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
        if (descriptor?.VisemeSkinnedMesh is { } v)
            return v;
#endif

        return avatarRoot.GetComponentInChildren<ModEmoFaceObject>()?.Renderer ?? avatarRoot.Find("Body")?.GetComponent<SkinnedMeshRenderer>();
    }
}