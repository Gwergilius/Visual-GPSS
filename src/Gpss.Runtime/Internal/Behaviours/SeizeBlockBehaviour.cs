using Gpss.Model.Blocks;
using Microsoft.Extensions.Logging;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime.Internal.Behaviours;

/// <summary>
/// Implements the GPSS SEIZE block: claims exclusive ownership of a <see cref="Facility"/>.
/// If the Facility is busy the transaction is queued inside the Facility and returns
/// <see cref="BlockTransactionResult.Delayed"/>; it will be re-activated by
/// <see cref="ReleaseBlockBehaviour"/> when the Facility becomes free.
/// </summary>
internal sealed class SeizeBlockBehaviour(ILogger<SeizeBlockBehaviour> logger)
    : BlockBehaviour<SeizeBlock>
{
    /// <inheritdoc/>
    protected override void OnSimulationStart(SeizeBlock block, BlockContext blockContext, ISimulationContext context)
    {
        // No initialisation needed
    }

    /// <inheritdoc/>
    protected override BlockTransactionResult OnTransactionArrival(
        SeizeBlock block, BlockContext blockContext, Transaction tx, ISimulationContext context)
    {
        var facilityName = EvaluateName(block.FacilityName);
        var facility = context.GetOrCreateFacility(facilityName);

        if (facility.TrySeize(tx))
        {
            tx.BlockIndex = blockContext.Index + 1;
            logger.LogDebug(
                "SEIZE[{Index}]: tx #{TxId} seized '{Facility}' at t={Clock}",
                blockContext.Index, tx.Id, facilityName, context.Clock);
            return BlockTransactionResult.Moved;
        }

        facility.EnqueueWaiting(tx);
        logger.LogDebug(
            "SEIZE[{Index}]: tx #{TxId} queued for '{Facility}' (queue={Queue}) at t={Clock}",
            blockContext.Index, tx.Id, facilityName, facility.WaitCount, context.Clock);
        return BlockTransactionResult.Delayed;
    }
}
