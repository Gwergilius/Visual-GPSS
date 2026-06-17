using Gpss.Model.Variates;

namespace Gpss.Model.Blocks;

/// <summary>
/// Delays a transaction for a period of simulated time before it continues to the next block.
/// Corresponds to the GPSS ADVANCE block (operands A, B).
/// </summary>
/// <param name="DelayTime">
/// Operands A–B. Distribution describing the delay duration. A block built from source with
/// no operands resolves to a <see cref="ConstantVariateSpec"/> of zero.
/// </param>
public sealed record AdvanceBlock(
    VariateSpec DelayTime
) : GpssBlock(), IKnownGpssBlock
{
    /// <inheritdoc/>
    public static string Keyword => typeof(AdvanceBlock).DefaultGpssKeyword;
}
