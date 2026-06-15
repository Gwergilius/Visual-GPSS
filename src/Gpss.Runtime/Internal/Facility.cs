namespace Gpss.Runtime.Internal;

/// <summary>
/// A GPSS Facility — a single-server resource that can be owned by at most one
/// transaction at a time. Transactions that arrive while the Facility is busy wait
/// in an internal FIFO queue and are activated when the current owner releases it.
/// </summary>
internal sealed class Facility(string name)
{
    private Transaction? _owner;
    private readonly Queue<Transaction> _waitQueue = new();

    /// <summary>Name of the facility as declared in the GPSS source.</summary>
    internal string Name { get; } = name;

    /// <summary><see langword="true"/> when a transaction currently owns this facility.</summary>
    internal bool IsBusy => _owner is not null;

    /// <summary>Number of transactions currently waiting to seize this facility.</summary>
    internal int WaitCount => _waitQueue.Count;

    /// <summary>
    /// Attempts to seize this facility for <paramref name="tx"/>.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the facility was free and <paramref name="tx"/> is now
    /// the owner; <see langword="false"/> when the facility was already busy.
    /// </returns>
    internal bool TrySeize(Transaction tx)
    {
        if (IsBusy) return false;
        _owner = tx;
        return true;
    }

    /// <summary>Adds <paramref name="tx"/> to the end of the internal wait queue.</summary>
    internal void EnqueueWaiting(Transaction tx) => _waitQueue.Enqueue(tx);

    /// <summary>
    /// Releases this facility. If transactions are waiting, ownership is immediately
    /// transferred to the next one, which is returned. Returns <see langword="null"/>
    /// when no transactions were waiting and the facility becomes idle.
    /// </summary>
    internal Transaction? Release()
    {
        if (_waitQueue.TryDequeue(out var next))
        {
            _owner = next;
            return next;
        }

        _owner = null;
        return null;
    }
}
