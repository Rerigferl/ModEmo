namespace Numeira
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    [PreferBinarySerialization]
    public sealed class ModEmoGeneratedArtifactsCache : ScriptableObject
    {
        internal const string ArtifactCachePath = "Packages/numeira.mod-emo/__Generated/";

        public int HashCode;

        [SerializeField]
        private ulong ExpiresData;

#if UNITY_EDITOR
        public ref DateTime Expires => ref Unsafe.As<ulong, DateTime>(ref ExpiresData);

        static ModEmoGeneratedArtifactsCache()
        {
            EditorApplication.delayCall += () =>
            {
                var generatedArtifactPathes = System.IO.Directory.Exists(ArtifactCachePath) ? System.IO.Directory.GetFiles(ArtifactCachePath, "*.asset") : Array.Empty<string>();

                foreach (var generatedArtifact in generatedArtifactPathes)
                {
                    var x = AssetDatabase.LoadAssetAtPath<ModEmoGeneratedArtifactsCache>(generatedArtifact);
                    if (x == null)
                        continue;
                    if (x.Expires < DateTime.Now)
                        System.IO.File.Delete(generatedArtifact);
                }
            };
        }

        public static IEnumerable<ModEmoGeneratedArtifactsCache> GetStoredCaches()
        {
            var generatedArtifactPathes = System.IO.Directory.Exists(ArtifactCachePath) ? System.IO.Directory.GetFiles(ArtifactCachePath, "*.asset") : Array.Empty<string>();

            foreach (var generatedArtifact in generatedArtifactPathes)
            {
                var x = AssetDatabase.LoadAssetAtPath<ModEmoGeneratedArtifactsCache>(generatedArtifact);
                if (x != null)
                    yield return x;
            }
        }

        public static ModEmoGeneratedArtifactsCache? FindCache(int hashCode)
        {
            foreach (var cache in GetStoredCaches())
            {
                if (cache.HashCode == hashCode)
                    return cache;
            }
            return null;
        }
#endif
    }

#if UNITY_EDITOR
    static partial class RuntimeEditor
    {
        [CustomEditor(typeof(ModEmoGeneratedArtifactsCache))]
        internal sealed class ModEmoGeneratedArtifactsCacheEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(serializedObject.FindProperty("HashCode"));
                var rect = EditorGUILayout.GetControlRect();
                var data = serializedObject.FindProperty("ExpiresData");
                var dateTime = (target as ModEmoGeneratedArtifactsCache)!.Expires;
                EditorGUI.LabelField(rect, "Expires");
                rect.x += EditorGUIUtility.labelWidth;
                rect.width = (rect.width - EditorGUIUtility.labelWidth) / 3;

                EditorGUI.BeginChangeCheck();
                var year = EditorGUI.IntField(rect, dateTime.Year);
                rect.x += rect.width;
                var month = EditorGUI.IntField(rect, dateTime.Month);
                rect.x += rect.width;
                var day = EditorGUI.IntField(rect, dateTime.Day);
                if (EditorGUI.EndChangeCheck())
                {
                    dateTime = new DateTime(year, month, day);
                    data.ulongValue = (ulong)dateTime.ToBinary();
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
#endif
}
