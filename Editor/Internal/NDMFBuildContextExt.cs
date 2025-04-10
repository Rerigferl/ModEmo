namespace Numeira;

internal static class NDMFBuildContextExt
{
    public static ModEmoData GetData(this BuildContext context)
        => context.GetState(ModEmoData.Init);

    public static ModEmoPluginDefinition.ExtensionContext GetModEmoContext(this BuildContext context)
        => context.Extension<ModEmoPluginDefinition.ExtensionContext>();

    public static VirtualControllerContext GetVirtualControllerContext(this BuildContext context)
        => context.Extension<VirtualControllerContext>();
}
