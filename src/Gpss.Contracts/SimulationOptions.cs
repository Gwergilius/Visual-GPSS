namespace Gpss.Contracts;

/// <summary>Parameters that control a single simulation run.</summary>
/// <param name="TerminationCount">
/// Initial value of the termination counter (equivalent to the argument of the GPSS START statement).
/// The simulation stops when the counter reaches zero.
/// </param>
/// <param name="RandomSeed">
/// Seed for the random number generator used for stochastic operands (e.g. GENERATE spread).
/// When <see langword="null"/> a non-deterministic seed is used, making the run non-reproducible.
/// </param>
/// <param name="MaxEvents">
/// Safety limit on the total number of events processed.
/// Prevents infinite loops in models where the termination counter never reaches zero.
/// <see langword="null"/> means no limit.
/// </param>
public sealed record SimulationOptions(
    long TerminationCount,
    int? RandomSeed = null,
    long? MaxEvents = null
);
