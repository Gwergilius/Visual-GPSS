using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="SeizeBlock"/> from its parsed source statement.</summary>
internal sealed class SeizeBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(SeizeBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(GpssStatement statement, List<DiagnosticMessage> diagnostics) =>
        FacilityBlockBuilder.Build(statement, diagnostics, name => new SeizeBlock(new SymbolExpression(name)));
}
