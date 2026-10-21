namespace Numeira;

internal interface ISubComponentOrder
{
    public SubComponentOrder Order { get; }
}

internal enum SubComponentOrder
{
    Ascending,
    Descending,
}