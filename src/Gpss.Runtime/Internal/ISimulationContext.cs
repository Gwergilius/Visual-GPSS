namespace Gpss.Runtime.Internal;

/// <summary>
/// Exposes simulation state to <see cref="IBlockBehaviour"/> implementations.
/// This is the mediator interface through which behaviours interact with the engine.
/// </summary>
internal interface ISimulationContext
{
    /// <summary>Current value of the simulation clock.</summary>
    double Clock { get; }

    /// <summary><see langword="true"/> when the termination counter has reached zero.</summary>
    bool IsTerminated { get; }

    /// <summary>
    /// Creates a new transaction positioned at <paramref name="blockIndex"/>,
    /// ready to be scheduled into the Future Events Chain.
    /// Does not increment <c>TotalTransactionsCreated</c>; call <see cref="RecordTransactionCreated"/>
    /// when the transaction is actually activated (i.e. when it arrives at a GENERATE block).
    /// </summary>
    /// <param name="blockIndex">Zero-based index of the block where the transaction starts.</param>
    Transaction CreateTransaction(int blockIndex);

    /// <summary>
    /// Records that one transaction has been activated (entered the model at a GENERATE block).
    /// Increments <c>TotalTransactionsCreated</c>.
    /// </summary>
    void RecordTransactionCreated();

    /// <summary>Enqueues <paramref name="tx"/> into the Future Events Chain to fire at simulation time <paramref name="at"/>.</summary>
    /// <param name="tx">The transaction to schedule.</param>
    /// <param name="at">Absolute simulation time at which the transaction becomes active.</param>
    void ScheduleTransaction(Transaction tx, double at);

    /// <summary>
    /// Subtracts <paramref name="amount"/> from the termination counter.
    /// When the counter reaches zero the simulation ends after the current CEC pass.
    /// </summary>
    /// <param name="amount">Amount to subtract. Zero is a no-op.</param>
    void DecrementTerminationCounter(long amount);
}
