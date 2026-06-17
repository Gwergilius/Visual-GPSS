using Gpss.Model.Blocks;
using Microsoft.Extensions.Logging;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime.Internal.Behaviours;

/// <summary>
/// Implements the GPSS GENERATE block: creates transactions and injects them into the FEC
/// at regular intervals. Each arrival also schedules the next one, forming a self-sustaining chain.
/// </summary>
internal sealed class GenerateBlockBehaviour(ILogger<GenerateBlockBehaviour> logger, IRandomNumberGeneratorFactory randomFactory)
    : BlockBehaviour<GenerateBlock>
{
    private static readonly string BN = "GENERATE".PadRight(9);

    private sealed class State(long limit, IRandomVariateGenerator variate)
    {
        internal long GenerationCount;
        internal long Limit { get; } = limit;
        internal IRandomVariateGenerator Variate { get; } = variate;
    }

    /// <inheritdoc/>
    protected override void OnSimulationStart(GenerateBlock block, BlockContext blockContext, ISimulationContext context)
    {
        var variate = randomFactory.Create(block.InterArrivalTime);
        var firstOffset = block.FirstTransactionOffset is not null
            ? Evaluate(block.FirstTransactionOffset)
            : variate.Sample();
        var limit = block.GenerationLimit is not null ? (long)Evaluate(block.GenerationLimit) : 0L;

        blockContext.State = new State(limit, variate);

        var tx = context.CreateTransaction(blockContext.Index);
        context.ScheduleTransaction(tx, firstOffset);

        logger.LogDebug(
            "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} → FEC t={ScheduledTime:F0}{Description}",
            context.Clock, blockContext.Index, BN, tx.Id, firstOffset, DescriptionSuffix(block));
    }

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        GenerateBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var state = (State)blockContext.State!;
        state.GenerationCount++;

        var limit = state.Limit;

        context.RecordTransactionCreated();

        if (limit == 0 || state.GenerationCount < limit)
        {
            var nextTx = context.CreateTransaction(blockContext.Index);
            var nextTime = context.Clock + state.Variate.Sample();
            context.ScheduleTransaction(nextTx, nextTime);

            logger.LogDebug(
                "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} activated; #{NextId} → FEC t={NextTime:F0}{Description}",
                context.Clock, blockContext.Index, BN, tx.Id, nextTx.Id, nextTime, DescriptionSuffix(block));
        }
        else
        {
            logger.LogDebug(
                "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} activated (generation limit reached){Description}",
                context.Clock, blockContext.Index, BN, tx.Id, DescriptionSuffix(block));
        }

        tx.BlockIndex = blockContext.Index + 1;
        return BlockTransactionResult.Moved;
    }
}
