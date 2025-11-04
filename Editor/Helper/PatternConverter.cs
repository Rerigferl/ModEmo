namespace Numeira;

internal static class PatternConverter
{
    [MenuItem("GameObject/Preferred to Right Hand", true, int.MaxValue)]
    internal static bool Selector()
    {
        return Selection.activeGameObject?.GetComponent<IModEmoExpressionPattern>() != null;
    }

    [MenuItem("GameObject/Preferred to Right Hand", false, int.MaxValue)]
    internal static void RightHandPreference()
    {
        var patterns = Selection.activeGameObject.GetComponent<IModEmoExpressionPattern>();
        Dictionary<AnimatorParameterCondition, IModEmoExpression> dict = new();
        foreach (var expression in patterns.Expressions)
        {
            foreach (var conditions in expression.Conditions)
            {
                foreach (var condition in conditions)
                {
                    dict.TryAdd(condition, expression);
                }
            }
        }
    }
}