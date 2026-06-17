using Gpss.Contracts;
using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal;

/// <summary>
/// Builds a single concrete <see cref="GpssBlock"/> type from its label and parsed operand tokens.
/// Implementations are registered in <see cref="BlockBuilderRegistry"/> and invoked by
/// <see cref="GpssParser"/> — the Mediator — via <see cref="BlockBuilderRegistry.For"/>.
/// </summary>
internal interface IBlockBuilder
{
    /// <summary>The concrete <see cref="GpssBlock"/> type this builder produces.</summary>
    Type BlockType { get; }

    /// <summary>
    /// Builds the block from its label and operand tokens.
    /// </summary>
    /// <param name="label">Optional block label.</param>
    /// <param name="operands">
    /// Parsed operand tokens (A, B, C, ...); <see langword="null"/> marks a skipped slot (e.g. the
    /// middle slot in <c>10,,5</c>).
    /// </param>
    /// <param name="lineNumber">1-based source line number, used in diagnostic messages.</param>
    /// <param name="diagnostics">Collector for parse errors.</param>
    /// <returns>The built block, or <see langword="null"/> when a required operand is missing or invalid.</returns>
    GpssBlock? Build(string? label, IReadOnlyList<string?> operands, int lineNumber, List<DiagnosticMessage> diagnostics);
}
