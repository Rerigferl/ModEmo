namespace Numeira;

internal static class SpanExt 
{
    public static T? FirstOrDefault<T>(this Span<T> span) where T : struct
        => FirstOrDefault((ReadOnlySpan<T>)span);

    public static T? FirstOrDefault<T>(this ReadOnlySpan<T> span) where T : struct
    {
        if (span.IsEmpty)
            return default;
        return span[0];
    }
}

internal static class SpanExt2
{
    public static T? FirstOrDefault<T>(this Span<T> span) where T : class
        => FirstOrDefault((ReadOnlySpan<T>)span);

    public static T? FirstOrDefault<T>(this ReadOnlySpan<T> span) where T : class
    {
        if (span.IsEmpty)
            return default;
        return span[0];
    }
}