using Gpss.Model.Blocks;

namespace Gpss.Runtime.Internal;

/// <summary>
/// Holds the per-simulation-run state associated with a single block instance.
/// Created by <see cref="SimulationEngine"/> at the start of each run.
/// </summary>
internal sealed class BlockContext(GpssBlock block, int index)
{
    /// <summary>The block this context belongs to.</summary>
    internal GpssBlock Block { get; } = block;

    /// <summary>Zero-based position of the block within <see cref="GpssProgram.Blocks"/>.</summary>
    internal int Index { get; } = index;

    /// <summary>
    /// Behaviour-specific per-run state. Each <see cref="IBlockBehaviour"/> implementation
    /// is responsible for initialising and casting this field.
    /// </summary>
    internal object? State { get; set; }
}
