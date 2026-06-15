namespace Gpss.Runtime.Internal;

/// <summary>
/// Represents a moving entity in the simulation — the GPSS concept of a "transaction".
/// Transactions are created by GENERATE blocks, flow through the model, and are destroyed by TERMINATE blocks.
/// </summary>
/// <param name="id">Unique sequential identifier assigned by the engine at creation time.</param>
/// <param name="creationTime">Simulation clock value at the moment this transaction was created.</param>
internal sealed class Transaction(int id, double creationTime)
{
    /// <summary>Unique sequential identifier assigned by the engine at creation time.</summary>
    internal int Id { get; } = id;

    /// <summary>Simulation clock value at the moment this transaction was created.</summary>
    internal double CreationTime { get; } = creationTime;
}
