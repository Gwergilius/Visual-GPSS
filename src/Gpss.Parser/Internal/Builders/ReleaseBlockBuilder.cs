using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="ReleaseBlock"/> from its label and operand tokens.</summary>
internal sealed class ReleaseBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(ReleaseBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(string? label, IReadOnlyList<string?> operands, int lineNumber, List<DiagnosticMessage> diagnostics) =>
        FacilityBlockBuilder.Build(label, operands, lineNumber, diagnostics,
            name => new ReleaseBlock(new SymbolExpression(name)));
}
