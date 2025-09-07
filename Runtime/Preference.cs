using System.IO;
using System.Reflection;

namespace Numeira;

internal abstract class Preference<T> where T : Preference<T>, new()
{
    public static T Default => instance ??= new();
    private static T? instance;

    internal static string FilePath =>
#if UNITY_EDITOR
        filePath ??= (string)typeof(FilePathAttribute).GetProperty("filepath", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(typeof(T).GetCustomAttribute<FilePathAttribute>());
#else
        "";
#endif
    private static string? filePath;

    protected Preference() : this(FilePath) { }

    protected Preference(string filePath)
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
            return;
        EditorJsonUtility.FromJsonOverwrite(filePath, this);
#endif
    }

    public static void Save() => Default.Save(FilePath);

    public void Save(string filePath)
    {
#if UNITY_EDITOR
        if (filePath is null)
            return;

        var json = EditorJsonUtility.ToJson(this, true);
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        File.WriteAllText(filePath, json);
#endif
    }
}