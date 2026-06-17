using Gpss.Contracts;
using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds an <see cref="AdvanceBlock"/> from its label and operand tokens.</summary>
internal sealed class AdvanceBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(AdvanceBlock);

    /// <inheritdoc/>
    public GpssBlock Build(string? label, IReadOnlyList<string?> operands, int lineNumber, List<DiagnosticMessage> diagnostics) =>
        new AdvanceBlock(
            OperandParsing.Operand(operands, 0, lineNumber, "A", diagnostics),
            OperandParsing.Operand(operands, 1, lineNumber, "B", diagnostics))
        { Label = label };
}
