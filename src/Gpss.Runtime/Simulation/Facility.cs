namespace Gpss.Runtime.Simulation;

/// <summary>
/// A GPSS Facility — a single-server resource (captured via SEIZE / RELEASE).
/// </summary>
public sealed class Facility
{
    public string Name { get; }
    public bool IsSeized => _holder != null;
    public Transaction? Holder => _holder;

    // Statistics
    public long TotalSeizes { get; private set; }
    public double TotalBusyTime { get; private set; }
    private double _seizeTime;

    private Transaction? _holder;
    private readonly Queue<Transaction> _waitQueue = new();

    public Facility(string name) => Name = name;

    /// <summary>
    /// Attempts to seize the facility. Returns <c>true</c> if successful,
    /// <c>false</c> if the facility is busy (transaction is queued).
    /// </summary>
    public bool TrySeize(Transaction xact, double clock)
    {
        if (_holder == null)
        {
            _holder    = xact;
            _seizeTime = clock;
            TotalSeizes++;
            return true;
        }
        Enqueue(xact);
        return false;
    }

    /// <summary>
    /// Releases the facility. Returns the next waiting transaction (if any).
    /// </summary>
    public Transaction? Release(Transaction xact, double clock)
    {
        if (_holder != xact)
            throw new InvalidOperationException($"Transaction {xact.Id} does not hold facility '{Name}'.");

        TotalBusyTime += clock - _seizeTime;
        _holder = null;

        if (_waitQueue.Count == 0) return null;

        var next = _waitQueue.Dequeue();
        _holder    = next;
        _seizeTime = clock;
        TotalSeizes++;
        return next;
    }

    private void Enqueue(Transaction xact)
    {
        // Insert respecting priority (highest first)
        var list = _waitQueue.ToList();
        int idx = list.FindIndex(t => t.Priority < xact.Priority);
        if (idx < 0)
            _waitQueue.Enqueue(xact);
        else
        {
            // Rebuild with xact inserted at correct position
            _waitQueue.Clear();
            list.Insert(idx, xact);
            foreach (var t in list) _waitQueue.Enqueue(t);
        }
    }

    /// <summary>Fraction of time the facility was busy (0–1).</summary>
    public double Utilization(double totalTime) =>
        totalTime > 0 ? TotalBusyTime / totalTime : 0;
}
