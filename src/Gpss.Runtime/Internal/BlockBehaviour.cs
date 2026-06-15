using Gpss.Model.Blocks;

namespace Gpss.Runtime.Internal;

/// <summary>
/// Typed base class for block behaviours. Casts the untyped <see cref="GpssBlock"/>
/// arguments to <typeparamref name="TBlock"/> before dispatching to the strongly-typed overloads.
/// </summary>
/// <typeparam name="TBlock">The concrete block type this behaviour handles.</typeparam>
internal abstract class BlockBehaviour<TBlock> : IBlockBehaviour where TBlock : GpssBlock
{
    /// <inheritdoc/>
    public Type BlockType => typeof(TBlock);

    /// <inheritdoc/>
    public void OnSimulationStart(BlockContext blockContext, ISimulationContext context) =>
        OnSimulationStart((TBlock)blockContext.Block, blockContext, context);

    /// <inheritdoc/>
    public BlockTransactionResult OnTransactionArrival(BlockContext blockContext, Transaction tx, ISimulationContext context) =>
        OnTransactionArrival((TBlock)blockContext.Block, blockContext, tx, context);

    /// <summary>Typed version of <see cref="IBlockBehaviour.OnSimulationStart"/>.</summary>
    /// <param name="block">The strongly-typed block.</param>
    /// <param name="blockContext">Per-run context for the block instance.</param>
    /// <param name="context">Simulation state exposed by the mediator.</param>
    protected abstract void OnSimulationStart(TBlock block, BlockContext blockContext, ISimulationContext context);

    /// <summary>Typed version of <see cref="IBlockBehaviour.OnTransactionArrival"/>.</summary>
    /// <param name="block">The strongly-typed block.</param>
    /// <param name="blockContext">Per-run context for the block instance.</param>
    /// <param name="tx">The arriving transaction.</param>
    /// <param name="context">Simulation state exposed by the mediator.</param>
    protected abstract BlockTransactionResult OnTransactionArrival(TBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context);
}
