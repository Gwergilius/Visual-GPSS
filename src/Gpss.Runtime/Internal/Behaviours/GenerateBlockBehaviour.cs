using Gpss.Model.Blocks;
using Microsoft.Extensions.Logging;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime.Internal.Behaviours;

/// <summary>
/// Implements the GPSS GENERATE block: creates transactions and injects them into the FEC
/// at regular intervals. Each arrival also schedules the next one, forming a self-sustaining chain.
/// </summary>
internal sealed class GenerateBlockBehaviour(ILogger<GenerateBlockBehaviour> logger)
    : BlockBehaviour<GenerateBlock>
{
    private sealed class State(long limit)
    {
        internal long GenerationCount;
        internal long Limit { get; } = limit;
    }

    /// <inheritdoc/>
    protected override void OnSimulationStart(GenerateBlock block, BlockContext blockContext, ISimulationContext context)
    {
        var mean = Evaluate(block.MeanInterArrivalTime);
        var firstOffset = block.FirstTransactionOffset is not null
            ? Evaluate(block.FirstTransactionOffset)
            : mean;
        var limit = block.GenerationLimit is not null ? (long)Evaluate(block.GenerationLimit) : 0L;

        blockContext.State = new State(limit);

        var tx = context.CreateTransaction(blockContext.Index);
        context.ScheduleTransaction(tx, firstOffset);

        logger.LogDebug(
            "GENERATE[{Index}]: first transaction #{TxId} scheduled at t={Time}",
            blockContext.Index, tx.Id, firstOffset);
    }

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        GenerateBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var state = (State)blockContext.State!;
        state.GenerationCount++;

        var mean = Evaluate(block.MeanInterArrivalTime);
        var limit = state.Limit;

        // The arriving transaction is now officially activated in the model
        context.RecordTransactionCreated();

        // Schedule the next arrival unless the generation limit has been reached
        if (limit == 0 || state.GenerationCount < limit)
        {
            var nextTx = context.CreateTransaction(blockContext.Index);
            context.ScheduleTransaction(nextTx, context.Clock + mean);

            logger.LogDebug(
                "GENERATE[{Index}]: next transaction #{TxId} scheduled at t={Time}",
                blockContext.Index, nextTx.Id, context.Clock + mean);
        }

        // Move the current transaction to the next block
        tx.BlockIndex = blockContext.Index + 1;

        logger.LogDebug(
            "GENERATE[{Index}]: transaction #{TxId} moved to block {Next} at t={Clock}",
            blockContext.Index, tx.Id, tx.BlockIndex, context.Clock);

        return BlockTransactionResult.Moved;
    }
}
