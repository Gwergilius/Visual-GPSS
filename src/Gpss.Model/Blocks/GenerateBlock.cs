using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

/// <summary>
/// Creates transactions and injects them into the simulation at regular intervals.
/// Corresponds to the GPSS GENERATE block (operands A–E).
/// </summary>
/// <param name="MeanInterArrivalTime">
/// Operand A. Mean time between consecutive transaction arrivals.
/// Must be a non-negative expression.
/// </param>
/// <param name="Spread">
/// Operand B. Half-range of uniform random variation applied to the inter-arrival time.
/// The actual interval is sampled from [A−B, A+B]. Null means no variation.
/// </param>
/// <param name="FirstTransactionOffset">
/// Operand C. Delay before the first transaction is created.
/// Defaults to <see cref="MeanInterArrivalTime"/> when null.
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
    GpssExpression MeanInterArrivalTime,
    GpssExpression? Spread = null,
    GpssExpression? FirstTransactionOffset = null,
    GpssExpression? GenerationLimit = null,
    GpssExpression? Priority = null
) : GpssBlock(), IKnownGpssBlock
{
    /// <inheritdoc/>
    public static string Keyword => typeof(GenerateBlock).DefaultGpssKeyword;
}
