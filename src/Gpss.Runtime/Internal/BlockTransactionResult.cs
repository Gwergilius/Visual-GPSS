namespace Gpss.Runtime.Internal;

/// <summary>Outcome of a block processing a transaction.</summary>
internal enum BlockTransactionResult
{
    /// <summary>The transaction moved to the next block; processing continues in the current CEC pass.</summary>
    Moved,

    /// <summary>The transaction was scheduled into the FEC for a future time; processing suspends.</summary>
    Delayed,

    /// <summary>The transaction was destroyed; it is removed from the simulation.</summary>
    Destroyed
}
