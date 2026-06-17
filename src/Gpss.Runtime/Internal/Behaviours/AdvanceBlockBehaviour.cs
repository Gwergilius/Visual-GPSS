using Gpss.Model.Blocks;
using Microsoft.Extensions.Logging;

namespace Gpss.Runtime.Internal.Behaviours;

/// <summary>
/// Implements the GPSS ADVANCE block: delays the arriving transaction for a period of
/// simulated time before it continues to the next block. The transaction is taken out of
/// the current CEC pass and re-scheduled into the Future Events Chain to resume at
/// <c>Clock + delay</c>.
/// </summary>
internal sealed class AdvanceBlockBehaviour(ILogger<AdvanceBlockBehaviour> logger, IRandomNumberGeneratorFactory randomFactory)
    : BlockBehaviour<AdvanceBlock>
{
    private static readonly string BN = "ADVANCE".PadRight(9);

    private sealed class State(IRandomVariateGenerator variate)
    {
        internal IRandomVariateGenerator Variate { get; } = variate;
    }

    /// <inheritdoc/>
    protected override void OnSimulationStart(AdvanceBlock block, BlockContext blockContext, ISimulationContext context) =>
        blockContext.State = new State(randomFactory.Create(block.DelayTime));

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        AdvanceBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var state = (State)blockContext.State!;
        var resumeTime = context.Clock + state.Variate.Sample();

        tx.BlockIndex = blockContext.Index + 1;
        context.ScheduleTransaction(tx, resumeTime);

        logger.LogDebug(
            "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} advancing → FEC t={ResumeTime:F0}{Description}",
            context.Clock, blockContext.Index, BN, tx.Id, resumeTime, DescriptionSuffix(block));

        return BlockTransactionResult.Delayed;
    }
}
