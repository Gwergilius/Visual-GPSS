using Gpss.Model.Blocks;
using Microsoft.Extensions.Logging;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime.Internal.Behaviours;

/// <summary>
/// Implements the GPSS DEPART block: records a transaction's departure from a
/// <see cref="GpssQueue"/> and accumulates its wait time. Does not delay the transaction.
/// </summary>
internal sealed class DepartBlockBehaviour(ILogger<DepartBlockBehaviour> logger)
    : BlockBehaviour<DepartBlock>
{
    /// <inheritdoc/>
    protected override void OnSimulationStart(DepartBlock block, BlockContext blockContext, ISimulationContext context)
    {
        // No initialisation needed
    }

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        DepartBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var queueName = EvaluateName(block.QueueName);
        var queue = context.GetOrCreateQueue(queueName);
        queue.Depart(tx, context.Clock);

        logger.LogDebug(
            "DEPART[{Index}]: tx #{TxId} left '{Queue}' (count={Count}) at t={Clock}",
            blockContext.Index, tx.Id, queueName, queue.CurrentCount, context.Clock);

        tx.BlockIndex = blockContext.Index + 1;
        return BlockTransactionResult.Moved;
    }
}
