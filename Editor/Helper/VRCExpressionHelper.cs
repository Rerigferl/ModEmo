#if VRC_SDK_VRCSDK3
using System.Text;

namespace Numeira;

internal static class VRCExpressionHelper
{
    [MenuItem($"CONTEXT/{nameof(ModEmoExpressionPattern)}/Set Expression Name by Condition", false, 100)]
    internal static void SetExpressionNameByCondition(MenuCommand command)
    {
        if (command.context is not ModEmoExpressionPattern pattern)
            return;

        SetExpressionNameByCondition(pattern);
    }

    internal static void SetExpressionNameByCondition(IModEmoExpressionPattern pattern)
    {
        var expressions = pattern.Component.GetComponentsInChildren<IModEmoExpression>();
        Undo.SetCurrentGroupName("Set Expression Name by Condition");
        var uid = Undo.GetCurrentGroup();
        try
        {
            StringBuilder sb = new();
            foreach (var expression in expressions)
            {
                var conditions = expression.Component.GetComponents<ModEmoGestureCondition>();
                if (conditions.Length == 0)
                    continue;

                sb.Clear();
                
                Gesture? left = conditions.FirstOrDefault(x => x.Hand is Hand.Left)?.Gesture;
                Gesture? right = conditions.FirstOrDefault(x => x.Hand is Hand.Right)?.Gesture;

                if (conditions.FirstOrDefault(x => x.Hand is Hand.Both) is { } both)
                {
                    left = right = both.Gesture;
                }

                if (left is not null)
                {
                    sb.Append(left.Value);
                }

                if (right is not null)
                {
                    if (left is not null)
                    {
                        sb.Append(" + ");
                    }
                    sb.Append(right.Value);
                }

                var go = expression.GameObject;
                Undo.RecordObject(go, "Rename");
                go.name = sb.ToString();
                EditorUtility.SetDirty(go);
            }
        }
        finally
        {
            Undo.CollapseUndoOperations(uid);
        }
    }
}

#endif