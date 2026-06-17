using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="DepartBlock"/> from its parsed source statement.</summary>
internal sealed class DepartBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(DepartBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(GpssStatement statement, List<DiagnosticMessage> diagnostics) =>
        FacilityBlockBuilder.Build(statement, diagnostics, name => new DepartBlock(new SymbolExpression(name)));
}
