namespace Numeira;

internal interface IModEmoExpressionControl : IModEmoComponent
{
    public bool Visible
    {
        get => GameObject.hideFlags.HasFlag(HideFlags.HideInInspector);
        set
        {
            if (value)
                GameObject.hideFlags |= HideFlags.HideInInspector;
            else
                GameObject.hideFlags &= ~HideFlags.HideInInspector;
        }
    }

    public bool Enable
    {
        get => (Component as MonoBehaviour)!.enabled;
        set => (Component as MonoBehaviour)!.enabled = value;
    }
}
