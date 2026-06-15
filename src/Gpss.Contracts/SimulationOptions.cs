namespace Gpss.Contracts;

/// <summary>
/// Parameters that control a single simulation run.
/// Designed for use with the <c>IOptions&lt;SimulationOptions&gt;</c> pattern:
/// bind from <c>appsettings.json</c> via <c>services.Configure&lt;SimulationOptions&gt;(section)</c>.
/// </summary>
public sealed class SimulationOptions
{
    /// <summary>
    /// Initial value of the termination counter (equivalent to the argument of the GPSS START statement).
    /// The simulation stops when the counter reaches zero. Default: <c>1</c>.
    /// </summary>
    public long TerminationCount { get; set; } = 1;

    /// <summary>
    /// Seed for the random number generator used for stochastic operands (e.g. GENERATE spread).
    /// When <see langword="null"/> a non-deterministic seed is used, making the run non-reproducible.
    /// </summary>
    public int? RandomSeed { get; set; }

    /// <summary>
    /// Safety limit on the total number of events processed.
    /// Prevents infinite loops in models where the termination counter never reaches zero.
    /// <see langword="null"/> means no limit.
    /// </summary>
    public long? MaxEvents { get; set; }
}
