using Gpss.Contracts;
using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="GenerateBlock"/> from its parsed source statement.</summary>
internal sealed class GenerateBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(GenerateBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(GpssStatement statement, List<DiagnosticMessage> diagnostics)
    {
        if (statement.Operands.Count == 0 || statement.Operands[0] is null)
        {
            diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
                $"{GenerateBlock.Keyword} requires operand A (mean inter-arrival time).", statement));
            return null;
        }

        var mean = OperandParsing.ParseIntExpr(statement, statement.Operands[0]!, "A", diagnostics);
        if (mean is null) return null;

        var interArrivalTime = OperandParsing.VariateFrom(statement, mean, 1, "B", diagnostics);

        return new GenerateBlock(
            interArrivalTime,
            OperandParsing.Operand(statement, 2, "C", diagnostics),
            OperandParsing.Operand(statement, 3, "D", diagnostics),
            OperandParsing.Operand(statement, 4, "E", diagnostics))
        { Label = statement.Label, Description = statement.Comment };
    }
}
