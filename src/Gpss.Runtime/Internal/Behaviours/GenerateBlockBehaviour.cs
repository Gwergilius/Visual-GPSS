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
    private static readonly string BN = "GENERATE".PadRight(9);

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
            "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} → FEC t={ScheduledTime:F0}",
            context.Clock, blockContext.Index, BN, tx.Id, firstOffset);
    }

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        GenerateBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var state = (State)blockContext.State!;
        state.GenerationCount++;

        var mean = Evaluate(block.MeanInterArrivalTime);
        var limit = state.Limit;

        context.RecordTransactionCreated();

        if (limit == 0 || state.GenerationCount < limit)
        {
            var nextTx = context.CreateTransaction(blockContext.Index);
            var nextTime = context.Clock + mean;
            context.ScheduleTransaction(nextTx, nextTime);

            logger.LogDebug(
                "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} activated; #{NextId} → FEC t={NextTime:F0}",
                context.Clock, blockContext.Index, BN, tx.Id, nextTx.Id, nextTime);
        }
        else
        {
            logger.LogDebug(
                "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} activated (generation limit reached)",
                context.Clock, blockContext.Index, BN, tx.Id);
        }

        tx.BlockIndex = blockContext.Index + 1;
        return BlockTransactionResult.Moved;
    }
}
