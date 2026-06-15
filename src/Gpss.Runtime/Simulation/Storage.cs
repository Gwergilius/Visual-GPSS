namespace Gpss.Runtime.Simulation;

/// <summary>
/// A GPSS Storage — a multi-server resource (ENTER / LEAVE).
/// </summary>
public sealed class Storage
{
    public string Name     { get; }
    public int    Capacity { get; }
    public int    InUse    { get; private set; }
    public int    Available => Capacity - InUse;

    // Statistics
    public long   TotalEntries    { get; private set; }
    public double TotalContentTime { get; private set; }
    private double _lastChangeTime;

    private readonly Queue<Transaction> _waitQueue = new();

    public Storage(string name, int capacity)
    {
        Name     = name;
        Capacity = capacity;
    }

    /// <summary>
    /// Attempts to enter the storage using <paramref name="units"/> units.
    /// Returns <c>true</c> on success.
    /// </summary>
    public bool TryEnter(Transaction xact, double clock, int units = 1)
    {
        if (Available >= units)
        {
            AccumulateContent(clock);
            InUse += units;
            TotalEntries++;
            return true;
        }
        _waitQueue.Enqueue(xact);
        return false;
    }

    /// <summary>
    /// Releases <paramref name="units"/> units.
    /// Returns transactions that can now be admitted (may be empty).
    /// </summary>
    public IEnumerable<Transaction> Leave(double clock, int units = 1)
    {
        AccumulateContent(clock);
        InUse -= units;
        if (InUse < 0) InUse = 0;

        var admitted = new List<Transaction>();
        while (_waitQueue.Count > 0 && Available >= 1)
        {
            var next = _waitQueue.Dequeue();
            AccumulateContent(clock);
            InUse++;
            TotalEntries++;
            admitted.Add(next);
        }
        return admitted;
    }

    private void AccumulateContent(double clock)
    {
        TotalContentTime += InUse * (clock - _lastChangeTime);
        _lastChangeTime   = clock;
    }

    /// <summary>Average content over time.</summary>
    public double AverageContent(double totalTime) =>
        totalTime > 0 ? TotalContentTime / totalTime : 0;

    /// <summary>Utilization as fraction of capacity.</summary>
    public double Utilization(double totalTime) =>
        Capacity > 0 ? AverageContent(totalTime) / Capacity : 0;
}
