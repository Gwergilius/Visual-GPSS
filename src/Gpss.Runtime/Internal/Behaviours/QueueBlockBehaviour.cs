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
    private static readonly string BN = "QUEUE".PadRight(9);

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
            "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} entered '{QueueName}'{Description}",
            context.Clock, blockContext.Index, BN, tx.Id, queueName, DescriptionSuffix(block));

        tx.BlockIndex = blockContext.Index + 1;
        return BlockTransactionResult.Moved;
    }
}
