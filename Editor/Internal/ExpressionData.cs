namespace Numeira;

internal sealed class ExpressionData
{
    public readonly IModEmoExpressionPattern Pattern;
    public readonly int PatternIndex;
    public readonly IModEmoExpression Expression;
    public readonly string? Id;
    public int Index;

    public ExpressionData(IModEmoExpressionPattern pattern, int patternIndex, IModEmoExpression expression, string? id)
    {
        Pattern = pattern;
        PatternIndex = patternIndex;
        Expression = expression;
        Id = id;
    }
}
