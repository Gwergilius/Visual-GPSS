using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

/// <summary>
/// Removes a transaction from the simulation and optionally decrements the termination counter.
/// Corresponds to the GPSS TERMINATE block (operand A).
/// </summary>
/// <param name="DecrementCount">
/// Operand A. Amount subtracted from the termination counter when a transaction passes through.
/// Null or zero means the transaction is destroyed without affecting the counter.
/// </param>
public sealed record TerminateBlock(
    GpssExpression? DecrementCount = null
) : GpssBlock();
