namespace Numeira;

[FilePath("numeira/ModEmoPreviewPrefs.asset", FilePathAttribute.Location.ProjectFolder)]
public class ModEmoPreviewPrefs : ScriptableSingleton<ModEmoPreviewPrefs>
{
    public bool AutoPlay = true;
    public float FrameTime = 0f;
}
