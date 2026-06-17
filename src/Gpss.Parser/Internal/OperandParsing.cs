using Gpss.Contracts;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal;

/// <summary>
/// Shared operand-parsing helpers used by <see cref="IBlockBuilder"/> implementations.
/// </summary>
internal static class OperandParsing
{
    /// <summary>
    /// Reads operand <paramref name="index"/> and parses it as an integer expression, or returns
    /// <see langword="null"/> when the operand slot is absent or was skipped (e.g. <c>10,,5</c>).
    /// </summary>
    /// <param name="operands">Parsed operand tokens.</param>
    /// <param name="index">Zero-based operand index (A=0, B=1, ...).</param>
    /// <param name="lineNumber">1-based source line number, used in diagnostic messages.</param>
    /// <param name="name">Operand letter (e.g. <c>"A"</c>), used in diagnostic messages.</param>
    /// <param name="diagnostics">Collector for parse errors.</param>
    internal static GpssExpression? Operand(
        IReadOnlyList<string?> operands, int index,
        int lineNumber, string name, List<DiagnosticMessage> diagnostics) =>
        index < operands.Count && operands[index] is { } text
            ? ParseIntExpr(text, lineNumber, name, diagnostics)
            : null;

    /// <summary>
    /// Parses <paramref name="text"/> as an <see cref="IntegerExpression"/>, recording a
    /// diagnostic error when it is not a valid integer.
    /// </summary>
    /// <param name="text">The operand text to parse.</param>
    /// <param name="lineNumber">1-based source line number, used in diagnostic messages.</param>
    /// <param name="operandName">Operand letter (e.g. <c>"A"</c>), used in diagnostic messages.</param>
    /// <param name="diagnostics">Collector for parse errors.</param>
    internal static IntegerExpression? ParseIntExpr(
        string text, int lineNumber, string operandName,
        List<DiagnosticMessage> diagnostics)
    {
        if (int.TryParse(text, out var value)) return new IntegerExpression(value);

        diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
            $"Line {lineNumber}: operand {operandName} '{text}' is not a valid integer."));
        return null;
    }
}
