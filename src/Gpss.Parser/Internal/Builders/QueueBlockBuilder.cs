using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="QueueBlock"/> from its parsed source statement.</summary>
internal sealed class QueueBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(QueueBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(GpssStatement statement, List<DiagnosticMessage> diagnostics) =>
        FacilityBlockBuilder.Build(statement, diagnostics, name => new QueueBlock(new SymbolExpression(name)));
}
