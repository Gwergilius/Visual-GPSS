using Gpss.Contracts;
using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal.Builders;

/// <summary>
/// Shared construction logic for facility-style blocks (SEIZE, RELEASE, QUEUE, DEPART), whose
/// only operand is a symbolic name. Each block type's <see cref="IBlockBuilder"/> is a thin
/// wrapper that calls <see cref="Build{TBlock}"/> with its own factory, so every block type
/// still gets its own auto-discoverable builder class.
/// </summary>
internal static class FacilityBlockBuilder
{
    /// <summary>Builds a facility-style block from operand A (the symbolic name).</summary>
    /// <typeparam name="TBlock">The concrete block type to produce.</typeparam>
    /// <param name="statement">The decoded statement being built.</param>
    /// <param name="diagnostics">Collector for parse errors.</param>
    /// <param name="factory">Creates the block from operand A (the symbolic name).</param>
    /// <returns>The built block, or <see langword="null"/> when operand A is missing.</returns>
    internal static TBlock? Build<TBlock>(
        GpssStatement statement, List<DiagnosticMessage> diagnostics, Func<string, TBlock> factory)
        where TBlock : GpssBlock, IKnownGpssBlock
    {
        if (statement.Operands.Count == 0 || statement.Operands[0] is null)
        {
            diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
                $"{TBlock.Keyword} requires operand A (facility name).", statement));
            return null;
        }

        return (TBlock)((GpssBlock)factory(statement.Operands[0]!)
            with { Label = statement.Label, Description = statement.Comment });
    }
}
