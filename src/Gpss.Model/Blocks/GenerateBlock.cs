using Gpss.Model.Expressions;
using Gpss.Model.Variates;

namespace Gpss.Model.Blocks;

/// <summary>
/// Creates transactions and injects them into the simulation at regular intervals.
/// Corresponds to the GPSS GENERATE block (operands A–E).
/// </summary>
/// <param name="InterArrivalTime">
/// Operands A–B. Distribution describing the time between consecutive transaction arrivals.
/// </param>
/// <param name="FirstTransactionOffset">
/// Operand C. Delay before the first transaction is created.
/// Defaults to a sample of <see cref="InterArrivalTime"/> when null.
/// </param>
/// <param name="GenerationLimit">
/// Operand D. Maximum number of transactions this block will create.
/// Null or zero means unlimited.
/// </param>
/// <param name="Priority">
/// Operand E. Priority level assigned to each generated transaction.
/// Higher values indicate higher priority. Null defaults to zero.
/// </param>
public sealed record GenerateBlock(
    VariateSpec InterArrivalTime,
    GpssExpression? FirstTransactionOffset = null,
    GpssExpression? GenerationLimit = null,
    GpssExpression? Priority = null
) : GpssBlock(), IKnownGpssBlock
{
    /// <inheritdoc/>
    public static string Keyword => typeof(GenerateBlock).DefaultGpssKeyword;
}
