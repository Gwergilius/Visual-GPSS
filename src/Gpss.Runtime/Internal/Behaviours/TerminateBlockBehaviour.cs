using Gpss.Model.Blocks;
using Microsoft.Extensions.Logging;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime.Internal.Behaviours;

/// <summary>
/// Implements the GPSS TERMINATE block: destroys a transaction and optionally decrements
/// the termination counter. When the counter reaches zero the simulation ends.
/// </summary>
internal sealed class TerminateBlockBehaviour(ILogger<TerminateBlockBehaviour> logger)
    : BlockBehaviour<TerminateBlock>
{
    /// <inheritdoc/>
    protected override void OnSimulationStart(TerminateBlock block, BlockContext blockContext, ISimulationContext context)
    {
        // No initialisation needed for TERMINATE
    }

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        TerminateBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var decrement = block.DecrementCount is not null ? (long)Evaluate(block.DecrementCount) : 0L;
        context.DecrementTerminationCounter(decrement);

        logger.LogDebug(
            "TERMINATE[{Index}]: transaction #{TxId} destroyed, counter decremented by {Decrement} at t={Clock}",
            blockContext.Index, tx.Id, decrement, context.Clock);

        return BlockTransactionResult.Destroyed;
    }
}
