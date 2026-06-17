namespace Gpss.Contracts;

/// <summary>Severity level of a <see cref="DiagnosticMessage"/>.</summary>
public enum DiagnosticSeverity
{
    /// <summary>Informational message; does not affect the simulation outcome.</summary>
    Info,

    /// <summary>Non-fatal condition; the simulation completed but something unexpected occurred.</summary>
    Warning,

    /// <summary>Fatal condition; the simulation did not complete successfully.</summary>
    Error
}
