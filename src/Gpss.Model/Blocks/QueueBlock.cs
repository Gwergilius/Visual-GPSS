using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

/// <summary>
/// Records a transaction's entry into a user-defined Queue for statistical measurement.
/// Does not delay the transaction; it moves immediately to the next block.
/// Paired with <see cref="DepartBlock"/> to measure total waiting time.
/// Corresponds to the GPSS QUEUE block (operands A: queue name, B: units).
/// </summary>
/// <param name="QueueName">Operand A. Symbolic name of the Queue to enter.</param>
/// <param name="Count">
/// Operand B. Number of units added to the queue count. Defaults to 1 when <see langword="null"/>.
/// </param>
public sealed record QueueBlock(
    GpssExpression QueueName,
    GpssExpression? Count = null
) : GpssBlock(), IKnownGpssBlock
{
    /// <inheritdoc/>
    public static string Keyword => typeof(QueueBlock).DefaultGpssKeyword;
}
