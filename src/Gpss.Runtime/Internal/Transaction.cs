namespace Gpss.Runtime.Internal;

internal sealed class Transaction(int id, double creationTime)
{
    internal int Id { get; } = id;
    internal double CreationTime { get; } = creationTime;
}
