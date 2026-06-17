using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

/// <summary>
/// Delays a transaction for a period of simulated time before it continues to the next block.
/// Corresponds to the GPSS ADVANCE block (operands A, B).
/// </summary>
/// <param name="MeanDelayTime">
/// Operand A. Mean duration of the delay. Null or zero means no delay.
/// </param>
/// <param name="Spread">
/// Operand B. Half-range of uniform random variation applied to the delay.
/// The actual duration is sampled from [A−B, A+B]. Null means no variation.
/// </param>
public sealed record AdvanceBlock(
    GpssExpression? MeanDelayTime = null,
    GpssExpression? Spread = null
) : GpssBlock(), IKnownGpssBlock
{
    /// <inheritdoc/>
    public static string Keyword => typeof(AdvanceBlock).DefaultGpssKeyword;
}
