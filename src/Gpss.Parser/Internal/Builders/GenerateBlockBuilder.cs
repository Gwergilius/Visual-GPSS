using Gpss.Contracts;
using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="GenerateBlock"/> from its label and operand tokens.</summary>
internal sealed class GenerateBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(GenerateBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(string? label, IReadOnlyList<string?> operands, int lineNumber, List<DiagnosticMessage> diagnostics)
    {
        if (operands.Count == 0 || operands[0] is null)
        {
            diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
                $"Line {lineNumber}: {GenerateBlock.Keyword} requires operand A (mean inter-arrival time)."));
            return null;
        }

        var mean = OperandParsing.ParseIntExpr(operands[0]!, lineNumber, "A", diagnostics);
        if (mean is null) return null;

        return new GenerateBlock(
            mean,
            OperandParsing.Operand(operands, 1, lineNumber, "B", diagnostics),
            OperandParsing.Operand(operands, 2, lineNumber, "C", diagnostics),
            OperandParsing.Operand(operands, 3, lineNumber, "D", diagnostics),
            OperandParsing.Operand(operands, 4, lineNumber, "E", diagnostics))
        { Label = label };
    }
}
