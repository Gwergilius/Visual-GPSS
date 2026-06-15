using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

/// <summary>
/// Records a transaction's departure from a user-defined Queue and accumulates wait-time statistics.
/// Does not delay the transaction; it moves immediately to the next block.
/// Paired with <see cref="QueueBlock"/> to measure total waiting time.
/// Corresponds to the GPSS DEPART block (operands A: queue name, B: units).
/// </summary>
/// <param name="QueueName">Operand A. Symbolic name of the Queue to leave.</param>
/// <param name="Count">
/// Operand B. Number of units removed from the queue count. Defaults to 1 when <see langword="null"/>.
/// </param>
public sealed record DepartBlock(
    GpssExpression QueueName,
    GpssExpression? Count = null
) : GpssBlock();
