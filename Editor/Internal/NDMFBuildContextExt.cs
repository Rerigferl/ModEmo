namespace Numeira;

internal static class NDMFBuildContextExt
{
    public static ModEmoData GetData(this BuildContext context)
        => context.GetState(ModEmoData.Init);

    public static ModEmoPluginDefinition.ModEmoContext GetModEmoContext(this BuildContext context)
        => context.Extension<ModEmoPluginDefinition.ModEmoContext>();

    public static VirtualControllerContext GetVirtualControllerContext(this BuildContext context)
        => context.Extension<VirtualControllerContext>();
}
