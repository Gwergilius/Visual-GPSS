namespace Gpss.Contracts;

/// <summary>A diagnostic message produced during parsing or simulation.</summary>
/// <param name="Severity">How serious the condition is.</param>
/// <param name="Message">Human-readable description of the condition.</param>
public sealed record DiagnosticMessage(DiagnosticSeverity Severity, string Message);
