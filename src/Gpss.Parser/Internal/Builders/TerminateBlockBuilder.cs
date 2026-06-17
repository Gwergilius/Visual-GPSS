using Gpss.Contracts;
using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="TerminateBlock"/> from its parsed source statement.</summary>
internal sealed class TerminateBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(TerminateBlock);

    /// <inheritdoc/>
    public GpssBlock Build(GpssStatement statement, List<DiagnosticMessage> diagnostics) =>
        new TerminateBlock(OperandParsing.Operand(statement, 0, "A", diagnostics))
        { Label = statement.Label, Description = statement.Comment };
}
