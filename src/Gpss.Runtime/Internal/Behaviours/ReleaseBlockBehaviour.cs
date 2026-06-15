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
            // nextTx.BlockIndex points to the SEIZE block where it was delayed;
            // advance it past SEIZE so it continues from the block after SEIZE.
            nextTx.BlockIndex++;
            context.ScheduleTransaction(nextTx, context.Clock);

            logger.LogDebug(
                "RELEASE[{Index}]: '{Facility}' transferred to tx #{NextId} at t={Clock}",
                blockContext.Index, facilityName, nextTx.Id, context.Clock);
        }
        else
        {
            logger.LogDebug(
                "RELEASE[{Index}]: '{Facility}' is now idle at t={Clock}",
                blockContext.Index, facilityName, context.Clock);
        }

        tx.BlockIndex = blockContext.Index + 1;
        return BlockTransactionResult.Moved;
    }
}
