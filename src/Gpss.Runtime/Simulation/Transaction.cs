namespace Gpss.Runtime.Simulation;

/// <summary>
/// A GPSS Transaction — the fundamental moving entity in a simulation.
/// </summary>
public sealed class Transaction
{
    private static long _nextId = 1;

    /// <summary>Unique transaction serial number.</summary>
    public long Id { get; } = System.Threading.Interlocked.Increment(ref _nextId);

    /// <summary>Priority (higher = moves first). Default is 0.</summary>
    public int Priority { get; set; }

    /// <summary>The simulation clock time at which this transaction becomes active.</summary>
    public double MoveTime { get; set; }

    /// <summary>The block index this transaction is currently at (or about to enter).</summary>
    public int CurrentBlock { get; set; }

    /// <summary>Mark time: set by the MARK block, used for transit-time statistics.</summary>
    public double MarkTime { get; set; }

    /// <summary>Transaction parameters (P1…P12 in classic GPSS, unlimited here).</summary>
    public Dictionary<int, double> Parameters { get; } = new();

    /// <summary>User-defined attributes stored by name.</summary>
    public Dictionary<string, double> Attributes { get; } = new();

    public override string ToString() => $"Xact#{Id}[P={Priority},T={MoveTime:F2}]";
}
