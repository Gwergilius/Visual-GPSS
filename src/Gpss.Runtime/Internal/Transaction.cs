namespace Gpss.Runtime.Internal;

/// <summary>
/// Represents a moving entity in the simulation — the GPSS concept of a "transaction".
/// Transactions are created by GENERATE blocks, flow through the model, and are destroyed by TERMINATE blocks.
/// </summary>
/// <param name="id">Unique sequential identifier assigned by the engine at creation time.</param>
/// <param name="creationTime">Simulation clock value at the moment this transaction was created.</param>
internal sealed class Transaction(int id, double creationTime)
{
    /// <summary>Unique sequential identifier assigned by <see cref="ISimulationContext.CreateTransaction"/>.</summary>
    internal int Id { get; } = id;

    /// <summary>Simulation clock value at the moment this transaction was created.</summary>
    internal double CreationTime { get; } = creationTime;

    /// <summary>
    /// Zero-based index into <c>GpssProgram.Blocks</c> identifying the block
    /// where this transaction will be processed next.
    /// </summary>
    internal int BlockIndex { get; set; }

    /// <summary>
    /// Absolute simulation time at which this transaction becomes active.
    /// Set by <see cref="ISimulationContext.ScheduleTransaction"/> before enqueuing into the FEC.
    /// </summary>
    internal double ScheduledTime { get; set; } = creationTime;
}
