using System.Diagnostics.CodeAnalysis;

namespace Gpss.Runtime.Internal;

/// <summary>
/// Concrete per-run simulation state: Future Events Chain, clock, counters.
/// Created fresh by <see cref="SimulationEngine"/> for each call to <c>Run</c>.
/// </summary>
internal sealed class SimulationContext(long terminationCounter) : ISimulationContext
{
    // (time, insertion-sequence) priority guarantees FIFO ordering for simultaneous events
    private readonly PriorityQueue<Transaction, (double Time, int Seq)> _fec = new();
    private readonly Dictionary<string, Facility> _facilities = new(StringComparer.OrdinalIgnoreCase);
    private long _terminationCounter = terminationCounter;
    private int _nextTxId;
    private int _eventSeq;

    /// <inheritdoc/>
    public double Clock { get; private set; }

    /// <inheritdoc/>
    public bool IsTerminated => _terminationCounter <= 0;

    /// <summary>Total transactions created across all GENERATE blocks during this run.</summary>
    internal long TotalTransactionsCreated { get; private set; }

    /// <summary>Total transactions destroyed by TERMINATE blocks during this run.</summary>
    internal long TotalTransactionsTerminated { get; private set; }

    /// <inheritdoc/>
    public Transaction CreateTransaction(int blockIndex) =>
        new(++_nextTxId, Clock) { BlockIndex = blockIndex };

    /// <inheritdoc/>
    public void RecordTransactionCreated() => TotalTransactionsCreated++;

    /// <inheritdoc/>
    public void ScheduleTransaction(Transaction tx, double at)
    {
        tx.ScheduledTime = at;
        _fec.Enqueue(tx, (at, _eventSeq++));
    }

    /// <inheritdoc/>
    public void DecrementTerminationCounter(long amount)
    {
        _terminationCounter -= amount;
        TotalTransactionsTerminated++;
    }

    /// <inheritdoc/>
    public Facility GetOrCreateFacility(string name)
    {
        if (!_facilities.TryGetValue(name, out var facility))
            _facilities[name] = facility = new Facility(name);
        return facility;
    }

    /// <summary>Advances the simulation clock to <paramref name="time"/>.</summary>
    internal void AdvanceClock(double time) => Clock = time;

    /// <summary>Attempts to dequeue the next transaction from the Future Events Chain.</summary>
    /// <param name="tx">The dequeued transaction, or <see langword="null"/> if the FEC is empty.</param>
    /// <returns><see langword="true"/> if a transaction was available.</returns>
    internal bool TryDequeueNext([NotNullWhen(true)] out Transaction? tx) =>
        _fec.TryDequeue(out tx, out _);
}
