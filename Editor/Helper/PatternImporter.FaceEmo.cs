#if FACE_EMO
using Suzuryg.FaceEmo.Components;
using Suzuryg.FaceEmo.Components.Data;
#endif

namespace Numeira;

static partial class PatternImporter
{
#if FACE_EMO
    public static GameObject[]? ImportFromFaceEmo(GameObject faceEmoRoot)
    {
        if (!faceEmoRoot.TryGetComponent<MenuRepositoryComponent>(out var repository))
            return null;

        var groups = repository.SerializableMenu.Registered.Modes.AsSpan();

        var result = new GameObject[groups.Length];

        for(int i = 0; i < groups.Length; i++)
        {
            var group = groups[i];
            var groupName = group.DisplayName;
            var go = new GameObject(groupName);
            result[i] = go;
            go.AddComponent<ModEmoExpressionPattern>();

            int count = 0;
            foreach(var branch in group.Branches.AsSpan())
            {
                var exp = new GameObject();
                exp.name = $"Expression {count + 1}";

                bool isMultiframe = branch.IsLeftTriggerUsed || branch.IsRightTriggerUsed;

                if (isMultiframe)
                {

                }
                else
                {
                    var c = exp.AddComponent<ModEmoAnimationClipExpression>();

                }

                exp.transform.parent = go.transform;

                count++;
            }
        }


        return result;
    }
#else
    public static GameObject[]? ImportFromFaceEmo(GameObject faceEmoRoot)
    {
        return null;
    }
#endif

    private static T? LoadFromGUID<T>(string guid) where T : Object
    {
        return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guid));
    }
}

