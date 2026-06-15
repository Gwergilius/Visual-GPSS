using Gpss.Model.Blocks;
using Microsoft.Extensions.Logging;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime.Internal.Behaviours;

/// <summary>
/// Implements the GPSS QUEUE block: records a transaction's entry into a
/// <see cref="GpssQueue"/> for statistical measurement. Does not delay the transaction.
/// </summary>
internal sealed class QueueBlockBehaviour(ILogger<QueueBlockBehaviour> logger)
    : BlockBehaviour<QueueBlock>
{
    /// <inheritdoc/>
    protected override void OnSimulationStart(QueueBlock block, BlockContext blockContext, ISimulationContext context)
    {
        // No initialisation needed
    }

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        QueueBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var queueName = EvaluateName(block.QueueName);
        var queue = context.GetOrCreateQueue(queueName);
        queue.Enter(tx, context.Clock);

        logger.LogDebug(
            "QUEUE[{Index}]: tx #{TxId} entered '{Queue}' (count={Count}) at t={Clock}",
            blockContext.Index, tx.Id, queueName, queue.CurrentCount, context.Clock);

        tx.BlockIndex = blockContext.Index + 1;
        return BlockTransactionResult.Moved;
    }
}
