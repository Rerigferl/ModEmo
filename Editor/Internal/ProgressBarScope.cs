namespace Numeira;

internal ref struct ProgressBarScope
{
    private string title;
    private string info;
    private float progress;

    public ProgressBarScope(string title, string info, float progress = 0)
    {
        this.title = title;
        this.info = info;
        this.progress = progress;
        UpdateProgressBar();
    }

    public string Title
    {
        readonly get => title;
        set
        {
            title = value;
            UpdateProgressBar();
        }
    }

    public string Info
    {
        readonly get => info;
        set
        {
            info = value;
            UpdateProgressBar();
        }
    }

    public float Progress
    {
        readonly get => progress;
        set
        {
            progress = value;
            UpdateProgressBar();
        }
    }

    private readonly void UpdateProgressBar() => EditorUtility.DisplayProgressBar(title, info, progress);

    public readonly void Dispose()
    {
        EditorUtility.ClearProgressBar();
    }
}
