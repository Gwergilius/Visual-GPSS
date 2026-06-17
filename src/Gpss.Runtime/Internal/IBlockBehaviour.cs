using Gpss.Model.Blocks;

namespace Gpss.Runtime.Internal;

/// <summary>
/// Encapsulates the runtime behaviour of a single GPSS block type.
/// Implementations are registered in <see cref="BlockBehaviourRegistry"/> and invoked by
/// <see cref="SimulationEngine"/> — the Mediator — via <see cref="BlockBehaviourRegistry.For"/>.
/// </summary>
internal interface IBlockBehaviour
{
    /// <summary>The concrete <see cref="GpssBlock"/> type this behaviour handles.</summary>
    Type BlockType { get; }

    /// <summary>
    /// Called once per block instance at the start of each simulation run.
    /// Used for initialisation, e.g. scheduling the first transaction for a GENERATE block.
    /// </summary>
    /// <param name="blockContext">Per-run context for the block instance.</param>
    /// <param name="context">Simulation state exposed by the mediator.</param>
    void OnSimulationStart(BlockContext blockContext, ISimulationContext context);

    /// <summary>
    /// Called when a transaction arrives at this block during a CEC pass.
    /// </summary>
    /// <param name="blockContext">Per-run context for the block instance.</param>
    /// <param name="tx">The arriving transaction.</param>
    /// <param name="context">Simulation state exposed by the mediator.</param>
    /// <returns>What happened to the transaction after this block processed it.</returns>
    BlockTransactionResult OnTransactionArrival(BlockContext blockContext, Transaction tx, ISimulationContext context);
}
