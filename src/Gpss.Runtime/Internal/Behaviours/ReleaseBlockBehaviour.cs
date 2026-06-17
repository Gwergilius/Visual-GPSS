using Gpss.Model.Blocks;
using Microsoft.Extensions.Logging;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime.Internal.Behaviours;

/// <summary>
/// Implements the GPSS RELEASE block: relinquishes ownership of a <see cref="Facility"/>.
/// If transactions are waiting, ownership is immediately transferred to the next one and
/// that transaction is re-activated in the FEC at the current clock time.
/// </summary>
internal sealed class ReleaseBlockBehaviour(ILogger<ReleaseBlockBehaviour> logger)
    : BlockBehaviour<ReleaseBlock>
{
    private static readonly string BN = "RELEASE".PadRight(9);

    /// <inheritdoc/>
    protected override void OnSimulationStart(ReleaseBlock block, BlockContext blockContext, ISimulationContext context)
    {
        // No initialisation needed
    }

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        ReleaseBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var facilityName = EvaluateName(block.FacilityName);
        var facility = context.GetOrCreateFacility(facilityName);

        var nextTx = facility.Release();

        if (nextTx is not null)
        {
            nextTx.BlockIndex++;
            context.ScheduleTransaction(nextTx, context.Clock);

            logger.LogDebug(
                "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} releases '{Facility}'; tx #{NextId} activated",
                context.Clock, blockContext.Index, BN, tx.Id, facilityName, nextTx.Id);
        }
        else
        {
            logger.LogDebug(
                "{SimTime,5:F0} [{BlockIndex}]{BlockName}: tx #{TxId} releases '{Facility}'",
                context.Clock, blockContext.Index, BN, tx.Id, facilityName);
        }

        tx.BlockIndex = blockContext.Index + 1;
        return BlockTransactionResult.Moved;
    }
}
