namespace Numeira;

internal interface IModEmoExpressionControl : IModEmoComponent
{
    public bool Visible
    {
        get => Component.hideFlags.HasFlag(HideFlags.HideInInspector);
        set
        {
            if (value)
                Component.hideFlags |= HideFlags.HideInInspector;
            else
                Component.hideFlags &= ~HideFlags.HideInInspector;
        }
    }

    public bool Enable
    {
        get => (Component as MonoBehaviour)!.enabled;
        set => (Component as MonoBehaviour)!.enabled = value;
    }
}

internal static class ExpressionControlExt
{
    public static T WithVisibile<T>(this T component, bool visible) where T : IModEmoExpressionControl
    {
        component.Visible = visible;
        return component;
    }
}