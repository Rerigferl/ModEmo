namespace Numeira;

internal interface ISubComponentOrder
{
    public SubComponentOrder SubComponentOrder { get; }
}

internal enum SubComponentOrder
{
    Ascending,
    Descending,
}