namespace Gpss.Runtime.Internal;

/// <summary>
/// A GPSS Queue — a statistical measurement entity that tracks how many transactions are
/// waiting between a QUEUE block and its matching DEPART block, and for how long.
/// Does not control transaction flow; all measurement is passive.
/// </summary>
internal sealed class GpssQueue(string name)
{
    private int _currentCount;
    private readonly Dictionary<int, double> _entryTimes = new();

    /// <summary>Name of the queue as declared in the GPSS source.</summary>
    internal string Name { get; } = name;

    /// <summary>Number of transactions currently between QUEUE and DEPART.</summary>
    internal int CurrentCount => _currentCount;

    /// <summary>Highest simultaneous transaction count observed during the run.</summary>
    internal int MaxCount { get; private set; }

    /// <summary>Total number of transactions that entered this queue.</summary>
    internal long TotalEntries { get; private set; }

    /// <summary>Cumulative wait time across all transactions that departed.</summary>
    internal double TotalWaitTime { get; private set; }

    /// <summary>Average time a transaction spent between QUEUE and DEPART.</summary>
    internal double AverageWaitTime =>
        TotalEntries > 0 ? TotalWaitTime / TotalEntries : 0.0;

    /// <summary>Records a transaction entering the queue at <paramref name="clock"/> time.</summary>
    /// <param name="tx">The entering transaction.</param>
    /// <param name="clock">Current simulation clock value.</param>
    internal void Enter(Transaction tx, double clock)
    {
        _entryTimes[tx.Id] = clock;
        _currentCount++;
        TotalEntries++;
        if (_currentCount > MaxCount)
            MaxCount = _currentCount;
    }

    /// <summary>
    /// Records a transaction departing the queue and accumulates its wait time.
    /// </summary>
    /// <param name="tx">The departing transaction.</param>
    /// <param name="clock">Current simulation clock value.</param>
    internal void Depart(Transaction tx, double clock)
    {
        if (_entryTimes.TryGetValue(tx.Id, out var entryTime))
        {
            TotalWaitTime += clock - entryTime;
            _entryTimes.Remove(tx.Id);
        }

        if (_currentCount > 0)
            _currentCount--;
    }
}
