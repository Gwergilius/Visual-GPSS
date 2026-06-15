namespace Gpss.Contracts;

/// <summary>Aggregate statistics collected during a simulation run.</summary>
/// <param name="SimulationEndTime">Simulation clock value at the moment the run ended.</param>
/// <param name="TotalTransactionsCreated">Total number of transactions created by all GENERATE blocks.</param>
/// <param name="TotalTransactionsTerminated">Total number of transactions destroyed by TERMINATE blocks.</param>
public sealed record SimulationStatistics(
    double SimulationEndTime,
    long TotalTransactionsCreated,
    long TotalTransactionsTerminated
);
