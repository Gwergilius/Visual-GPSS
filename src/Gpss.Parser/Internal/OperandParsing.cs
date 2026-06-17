using Gpss.Contracts;
using Gpss.Model.Expressions;
using Gpss.Model.Variates;

namespace Gpss.Parser.Internal;

/// <summary>
/// Shared operand-parsing helpers used by <see cref="IBlockBuilder"/> implementations.
/// </summary>
internal static class OperandParsing
{
    /// <summary>
    /// Reads operand <paramref name="index"/> from <paramref name="statement"/> and parses it as
    /// an integer expression, or returns <see langword="null"/> when the operand slot is absent
    /// or was skipped (e.g. <c>10,,5</c>).
    /// </summary>
    /// <param name="statement">The statement being built, used for its operands and source location.</param>
    /// <param name="index">Zero-based operand index (A=0, B=1, ...).</param>
    /// <param name="name">Operand letter (e.g. <c>"A"</c>), used in diagnostic messages.</param>
    /// <param name="diagnostics">Collector for parse errors.</param>
    internal static GpssExpression? Operand(
        GpssStatement statement, int index, string name, List<DiagnosticMessage> diagnostics) =>
        index < statement.Operands.Count && statement.Operands[index] is { } text
            ? ParseIntExpr(statement, text, name, diagnostics)
            : null;

    /// <summary>
    /// Builds a <see cref="VariateSpec"/> from a mean/spread operand pair: a
    /// <see cref="UniformVariateSpec"/> when the spread operand is present and valid, otherwise
    /// a <see cref="ConstantVariateSpec"/> wrapping <paramref name="mean"/>.
    /// </summary>
    /// <param name="statement">The statement being built, used for its operands and source location.</param>
    /// <param name="mean">The already-parsed mean expression.</param>
    /// <param name="spreadIndex">Zero-based index of the spread operand.</param>
    /// <param name="spreadName">Operand letter for the spread operand (e.g. <c>"B"</c>), used in diagnostic messages.</param>
    /// <param name="diagnostics">Collector for parse errors.</param>
    internal static VariateSpec VariateFrom(
        GpssStatement statement, GpssExpression mean, int spreadIndex, string spreadName,
        List<DiagnosticMessage> diagnostics)
    {
        var spread = Operand(statement, spreadIndex, spreadName, diagnostics);
        return spread is not null ? new UniformVariateSpec(mean, spread) : new ConstantVariateSpec(mean);
    }

    /// <summary>
    /// Parses <paramref name="text"/> as an <see cref="IntegerExpression"/>, recording a
    /// diagnostic error when it is not a valid integer.
    /// </summary>
    /// <param name="statement">The statement being built, used for its source location.</param>
    /// <param name="text">The operand text to parse.</param>
    /// <param name="operandName">Operand letter (e.g. <c>"A"</c>), used in diagnostic messages.</param>
    /// <param name="diagnostics">Collector for parse errors.</param>
    internal static IntegerExpression? ParseIntExpr(
        GpssStatement statement, string text, string operandName, List<DiagnosticMessage> diagnostics)
    {
        if (int.TryParse(text, out var value)) return new IntegerExpression(value);

        diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
            $"operand {operandName} '{text}' is not a valid integer.", statement));
        return null;
    }
}
