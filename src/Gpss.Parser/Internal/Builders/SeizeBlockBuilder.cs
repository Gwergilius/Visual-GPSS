using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser.Internal.Builders;

/// <summary>Builds a <see cref="SeizeBlock"/> from its label and operand tokens.</summary>
internal sealed class SeizeBlockBuilder : IBlockBuilder
{
    /// <inheritdoc/>
    public Type BlockType => typeof(SeizeBlock);

    /// <inheritdoc/>
    public GpssBlock? Build(string? label, IReadOnlyList<string?> operands, int lineNumber, List<DiagnosticMessage> diagnostics) =>
        FacilityBlockBuilder.Build(label, operands, lineNumber, diagnostics,
            name => new SeizeBlock(new SymbolExpression(name)));
}
