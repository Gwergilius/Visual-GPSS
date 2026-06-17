namespace Gpss.Contracts;

/// <summary>A diagnostic message produced during parsing or simulation.</summary>
/// <param name="Severity">How serious the condition is.</param>
/// <param name="Message">Human-readable description of the condition, without source location prefixed onto it.</param>
/// <param name="FileName">
/// Name or path of the source file the condition was found in, or <see langword="null"/> when no
/// specific file applies.
/// </param>
/// <param name="LineNumber">
/// 1-based source line number the condition was found at, or <see langword="null"/> when no
/// specific line applies.
/// </param>
/// <param name="Comment">
/// The source line's trailing comment, or <see langword="null"/> when it had none or no specific
/// line applies.
/// </param>
public sealed record DiagnosticMessage(
    DiagnosticSeverity Severity, string Message, string? FileName = null, int? LineNumber = null, string? Comment = null)
{
    /// <summary>
    /// Initializes a new instance, taking <see cref="FileName"/>, <see cref="LineNumber"/>, and
    /// <see cref="Comment"/> from <paramref name="location"/>.
    /// </summary>
    /// <param name="severity">How serious the condition is.</param>
    /// <param name="message">Human-readable description of the condition, without source location prefixed onto it.</param>
    /// <param name="location">The source location the condition was found at.</param>
    public DiagnosticMessage(DiagnosticSeverity severity, string message, ISourceLocation location)
        : this(severity, message, location.FileName, location.LineNumber, location.Comment)
    {
    }

    /// <summary>Renders the severity, source location (when known), message, and comment (when known) as a single line.</summary>
    public override string ToString()
    {
        var location = FileName switch
        {
            { Length: > 0 } when LineNumber is { } line => $"{FileName}({line}): ",
            { Length: > 0 } => $"{FileName}: ",
            _ when LineNumber is { } line => $"Line {line}: ",
            _ => "",
        };
        var commentSuffix = Comment is { Length: > 0 } ? $" ({Comment})" : "";

        return $"[{Severity}] {location}{Message}{commentSuffix}";
    }
}
