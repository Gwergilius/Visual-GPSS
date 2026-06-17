using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="ReleaseBlock"/> from its parsed source statement.</summary>
internal sealed class ReleaseBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(ReleaseBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(GpssStatement statement, List<DiagnosticMessage> diagnostics) =>
        FacilityBlockBuilder.Build(statement, diagnostics, name => new ReleaseBlock(new SymbolExpression(name)));
}
