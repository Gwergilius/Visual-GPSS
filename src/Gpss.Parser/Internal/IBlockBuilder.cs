using Gpss.Contracts;
using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal;

/// <summary>
/// Builds a single concrete <see cref="GpssBlock"/> type from its parsed source statement.
/// Implementations are registered in <see cref="BlockBuilderRegistry"/> and invoked by
/// <see cref="GpssParser"/> — the Mediator — via <see cref="BlockBuilderRegistry.For"/>.
/// </summary>
internal interface IBlockBuilder
{
    /// <summary>The concrete <see cref="GpssBlock"/> type this builder produces.</summary>
    Type BlockType { get; }

    /// <summary>
    /// Builds the block from its parsed source statement.
    /// </summary>
    /// <param name="statement">
    /// The decoded statement (label, operand tokens, file name, and line number) for this block.
    /// </param>
    /// <param name="diagnostics">Collector for parse errors.</param>
    /// <returns>The built block, or <see langword="null"/> when a required operand is missing or invalid.</returns>
    GpssBlock? Build(GpssStatement statement, List<DiagnosticMessage> diagnostics);
}
