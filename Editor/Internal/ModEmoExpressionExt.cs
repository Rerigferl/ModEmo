using Numeira.Animation;

namespace Numeira;

internal static partial class ModEmoExpressionExt
{
    public static string GetID<T>(this T expression) where T : IModEmoExpression
    {
        return $"{expression.Name}-{expression.GetHashCode()}";
    }
}
