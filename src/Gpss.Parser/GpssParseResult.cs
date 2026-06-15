using Gpss.Contracts;
using Gpss.Model;

namespace Gpss.Parser;

/// <summary>
/// The outcome of a <see cref="GpssParser.Parse"/> call.
/// Contains the parsed program when successful, and any diagnostics produced during parsing.
/// </summary>
/// <param name="Program">
/// The parsed <see cref="GpssProgram"/>, or <see langword="null"/> when parsing produced at least one error.
/// </param>
/// <param name="Diagnostics">
/// Errors, warnings, and informational messages produced during parsing.
/// </param>
public sealed record GpssParseResult(
    GpssProgram? Program,
    IReadOnlyList<DiagnosticMessage> Diagnostics)
{
    /// <summary>
    /// <see langword="true"/> when parsing completed without errors and a valid program was produced.
    /// </summary>
    public bool Success => Program is not null
        && !Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error);
}
