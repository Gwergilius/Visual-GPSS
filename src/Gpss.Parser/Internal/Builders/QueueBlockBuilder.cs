using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="QueueBlock"/> from its label and operand tokens.</summary>
internal sealed class QueueBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(QueueBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(string? label, IReadOnlyList<string?> operands, int lineNumber, List<DiagnosticMessage> diagnostics) =>
        FacilityBlockBuilder.Build(label, operands, lineNumber, diagnostics,
            name => new QueueBlock(new SymbolExpression(name)));
}
