namespace Gpss.Runtime.Simulation;

/// <summary>
/// A GPSS Queue — collects waiting-time statistics (QUEUE / DEPART blocks).
/// Note: this is a statistics collector, not a scheduling queue.
/// </summary>
public sealed class QueueEntity
{
    public string Name { get; }
    public int    CurrentCount { get; private set; }
    public int    MaxCount     { get; private set; }
    public long   TotalEntries { get; private set; }
    public long   ZeroEntries  { get; private set; }  // entries with 0 wait time
    public double TotalWait    { get; private set; }  // sum of all wait times

    private readonly Dictionary<long, double> _entryTimes = new();

    public QueueEntity(string name) => Name = name;

    /// <summary>Records a transaction joining the queue.</summary>
    public void Enter(Transaction xact, double clock)
    {
        CurrentCount++;
        TotalEntries++;
        if (CurrentCount > MaxCount) MaxCount = CurrentCount;
        _entryTimes[xact.Id] = clock;
    }

    /// <summary>Records a transaction leaving the queue.</summary>
    public void Depart(Transaction xact, double clock)
    {
        if (!_entryTimes.TryGetValue(xact.Id, out double enterTime))
            return;

        _entryTimes.Remove(xact.Id);
        double wait = clock - enterTime;
        TotalWait += wait;
        if (wait == 0) ZeroEntries++;
        CurrentCount = Math.Max(0, CurrentCount - 1);
    }

    /// <summary>Average wait time including zero-wait entries.</summary>
    public double AverageTime =>
        TotalEntries > 0 ? TotalWait / TotalEntries : 0;

    /// <summary>Average wait time excluding zero-wait entries.</summary>
    public double AverageTimeNonZero
    {
        get
        {
            long nonZero = TotalEntries - ZeroEntries;
            return nonZero > 0 ? TotalWait / nonZero : 0;
        }
    }
}
