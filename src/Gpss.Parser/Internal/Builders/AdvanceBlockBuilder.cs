using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds an <see cref="AdvanceBlock"/> from its parsed source statement.</summary>
internal sealed class AdvanceBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(AdvanceBlock);

    /// <inheritdoc/>
    public GpssBlock Build(GpssStatement statement, List<DiagnosticMessage> diagnostics)
    {
        var mean = OperandParsing.Operand(statement, 0, "A", diagnostics) ?? new IntegerExpression(0);
        var delayTime = OperandParsing.VariateFrom(statement, mean, 1, "B", diagnostics);

        return new AdvanceBlock(delayTime) { Label = statement.Label, Description = statement.Comment };
    }
}
