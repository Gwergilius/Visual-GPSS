namespace Gpss.Runtime.Simulation;

/// <summary>
/// A GPSS Table — frequency distribution of observed values.
/// Defined by the TABLE/QTABLE directive.
/// </summary>
public sealed class TableEntity
{
    public string Name    { get; }
    public double Lower   { get; }
    public double Width   { get; }
    public int    Count   { get; }

    public long   TotalEntries { get; private set; }
    public double Sum          { get; private set; }
    public double SumSquares   { get; private set; }

    private readonly long[] _frequencies;

    public TableEntity(string name, double lower, double width, int classCount)
    {
        Name  = name;
        Lower = lower;
        Width = width;
        Count = classCount;
        _frequencies = new long[classCount + 2]; // 0=underflow, 1..n=classes, n+1=overflow
    }

    /// <summary>Records an observation.</summary>
    public void Record(double value)
    {
        TotalEntries++;
        Sum        += value;
        SumSquares += value * value;

        int idx;
        if (value < Lower)
        {
            idx = 0; // underflow
        }
        else
        {
            idx = (int)((value - Lower) / Width) + 1;
            if (idx >= _frequencies.Length) idx = _frequencies.Length - 1; // overflow
        }
        _frequencies[idx]++;
    }

    public double Mean     => TotalEntries > 0 ? Sum / TotalEntries : 0;
    public double Variance => TotalEntries > 1
        ? (SumSquares - Sum * Sum / TotalEntries) / (TotalEntries - 1)
        : 0;
    public double StdDev   => Math.Sqrt(Variance);

    /// <summary>Returns the frequency array (index 0 = underflow, last = overflow).</summary>
    public IReadOnlyList<long> Frequencies => _frequencies;
}
