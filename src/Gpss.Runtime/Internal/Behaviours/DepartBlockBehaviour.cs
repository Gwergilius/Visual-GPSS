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
    private static readonly string BN = "DEPART".PadRight(9);

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
            "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} left '{QueueName}'",
            context.Clock, blockContext.Index, BN, tx.Id, queueName);

        tx.BlockIndex = blockContext.Index + 1;
        return BlockTransactionResult.Moved;
    }
}
