using System.Text;

namespace Gpss.Runtime.Simulation;

/// <summary>
/// Contains the statistical results produced at the end of a simulation run.
/// </summary>
public sealed class SimulationReport
{
    public double EndTime           { get; init; }
    public long   TerminateCount    { get; init; }
    public long   TotalTransactions { get; init; }

    public required IReadOnlyDictionary<string, Facility>    Facilities { get; init; }
    public required IReadOnlyDictionary<string, Storage>     Storages   { get; init; }
    public required IReadOnlyDictionary<string, QueueEntity> Queues     { get; init; }
    public required IReadOnlyDictionary<string, TableEntity> Tables     { get; init; }

    /// <summary>Formats a human-readable report similar to classic GPSS output.</summary>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine("─── GPSS SIMULATION REPORT ─────────────────────────────────────");
        sb.AppendLine($"  End Time          : {EndTime,12:F2}");
        sb.AppendLine($"  Terminate Count   : {TerminateCount,12}");
        sb.AppendLine($"  Total Transactions: {TotalTransactions,12}");
        sb.AppendLine();

        if (Facilities.Count > 0)
        {
            sb.AppendLine("FACILITY STATISTICS");
            sb.AppendLine($"  {"Name",-20} {"Entries",10} {"Util%",8} {"Busy Time",12}");
            sb.AppendLine($"  {"────────────────────",-20} {"──────────",10} {"────────",8} {"────────────",12}");
            foreach (var (name, f) in Facilities)
            {
                sb.AppendLine($"  {name,-20} {f.TotalSeizes,10} {f.Utilization(EndTime) * 100,8:F2} {f.TotalBusyTime,12:F2}");
            }
            sb.AppendLine();
        }

        if (Storages.Count > 0)
        {
            sb.AppendLine("STORAGE STATISTICS");
            sb.AppendLine($"  {"Name",-20} {"Cap",6} {"InUse",6} {"Entries",10} {"Util%",8} {"AvgContent",12}");
            sb.AppendLine($"  {"────────────────────",-20} {"──────",6} {"──────",6} {"──────────",10} {"────────",8} {"────────────",12}");
            foreach (var (name, s) in Storages)
            {
                sb.AppendLine($"  {name,-20} {s.Capacity,6} {s.InUse,6} {s.TotalEntries,10} {s.Utilization(EndTime) * 100,8:F2} {s.AverageContent(EndTime),12:F2}");
            }
            sb.AppendLine();
        }

        if (Queues.Count > 0)
        {
            sb.AppendLine("QUEUE STATISTICS");
            sb.AppendLine($"  {"Name",-20} {"MaxQ",6} {"Entries",10} {"AvgTime",10} {"AvgTimeNZ",10}");
            sb.AppendLine($"  {"────────────────────",-20} {"──────",6} {"──────────",10} {"──────────",10} {"──────────",10}");
            foreach (var (name, q) in Queues)
            {
                sb.AppendLine($"  {name,-20} {q.MaxCount,6} {q.TotalEntries,10} {q.AverageTime,10:F2} {q.AverageTimeNonZero,10:F2}");
            }
            sb.AppendLine();
        }

        if (Tables.Count > 0)
        {
            sb.AppendLine("TABLE STATISTICS");
            foreach (var (name, t) in Tables)
            {
                sb.AppendLine($"  Table: {name}  Entries={t.TotalEntries}  Mean={t.Mean:F2}  StdDev={t.StdDev:F2}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("────────────────────────────────────────────────────────────────");
        return sb.ToString();
    }
}
