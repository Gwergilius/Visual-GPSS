using Gpss.Contracts;
using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="TerminateBlock"/> from its label and operand tokens.</summary>
internal sealed class TerminateBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(TerminateBlock);

    /// <inheritdoc/>
    public GpssBlock Build(string? label, IReadOnlyList<string?> operands, int lineNumber, List<DiagnosticMessage> diagnostics) =>
        new TerminateBlock(OperandParsing.Operand(operands, 0, lineNumber, "A", diagnostics)) { Label = label };
}
