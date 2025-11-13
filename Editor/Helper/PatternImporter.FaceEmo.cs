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

        for (int i = 0; i < groups.Length; i++)
        {
            var group = groups[i];
            var groupName = group.DisplayName;
            var go = new GameObject(groupName);
            result[i] = go;
            go.AddComponent<ModEmoExpressionPattern>();

            int count = 0;
            foreach (var branch in group.Branches.AsSpan())
            {
                var exp = new GameObject();
                exp.name = $"Expression {count + 1}";

                var expression = exp.AddComponent<ModEmoDefaultExpression>();

                var blink = exp.AddComponent<ModEmoBlinkControl>();
                blink.enabled = branch.BlinkEnabled;

                var mmc = exp.AddComponent<ModEmoMouthMorphCancelControl>();
                mmc.enabled = branch.MouthMorphCancelerEnabled;

                foreach (var condition in branch.Conditions)
                {
                    if (condition.ComparisonOperator != Suzuryg.FaceEmo.Domain.ComparisonOperator.Equals)
                        continue;

                    switch(condition.Hand)
                    {
                        case Suzuryg.FaceEmo.Domain.Hand.Left or Suzuryg.FaceEmo.Domain.Hand.Right:
                            {
                                var cond = exp.AddComponent<ModEmoGestureCondition>();
                                cond.Hand = (Hand)(condition.Hand + 1);
                                cond.Gesture = (Gesture)condition.HandGesture;
                            }
                            break;
                        case Suzuryg.FaceEmo.Domain.Hand.Both:
                            {
                                var cond = exp.AddComponent<ModEmoGestureCondition>();
                                cond.Hand = Hand.Both;
                                cond.Gesture = (Gesture)condition.HandGesture;
                            }
                            break;
                    }
                }

                var blendShape = exp.AddComponent<ModEmoBlendShapeSelector>();
                blendShape.ImportFromAnimationClip(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(branch.BaseAnimation.GUID)));

                if (branch.IsLeftTriggerUsed)
                {
                    var blendShape2 = exp.AddComponent<ModEmoBlendShapeSelector>();
                    blendShape2.Keyframe = 1;
                    blendShape2.ImportFromAnimationClip(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(branch.LeftHandAnimation.GUID)));
                }

                if (branch.IsRightTriggerUsed)
                {
                    var blendShape2 = exp.AddComponent<ModEmoBlendShapeSelector>();
                    blendShape2.Keyframe = 1;
                    blendShape2.ImportFromAnimationClip(AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(branch.RightHandAnimation.GUID)));
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

