namespace Numeira;

internal interface IInspectorExtension
{
#if UNITY_EDITOR
    public void OnInspectorExtension(IInspectorExtensionContext context)
    {

    }
#endif
}

internal interface IInspectorExtensionContext
{
    public IDisposable BeginGroup(string label, ref bool open);
    public IDisposable BeginHorizontal();
    public IDisposable BeginVertical();
    public IDisposable BeginDisabled(bool disable);

    public void RegisterComponentShortcut(string title, Action<GameObject> callback);

    public bool CheckBox(string label, bool on, Action<bool>? callback = null);
    public bool Button(string label, Action? callback = null);
}