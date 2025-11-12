namespace System.Runtime.CompilerServices;

internal static class Disposable
{
    public static IDisposable Empty { get; } = new EmptyDisposable();

    private sealed class EmptyDisposable : IDisposable
    {
        public void Dispose()
        {
        }
    }
}