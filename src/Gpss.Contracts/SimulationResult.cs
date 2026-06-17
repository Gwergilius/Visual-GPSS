namespace Gpss.Contracts;

/// <summary>The outcome of a completed (or aborted) simulation run.</summary>
/// <param name="Success">
/// <see langword="true"/> when the simulation terminated normally (termination counter reached zero).
/// <see langword="false"/> when the run was aborted due to an error or the event limit was exceeded.
/// </param>
/// <param name="Statistics">Aggregate counters and clock value for the run.</param>
/// <param name="Diagnostics">Errors, warnings, and informational messages produced during the run.</param>
public sealed record SimulationResult(
    bool Success,
    SimulationStatistics Statistics,
    IReadOnlyList<DiagnosticMessage> Diagnostics
);
