using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

/// <summary>
/// Claims exclusive ownership of a Facility (single-server resource).
/// If the Facility is already busy, the transaction waits in its internal queue
/// until ownership can be transferred.
/// Corresponds to the GPSS SEIZE block (operand A: facility name).
/// </summary>
/// <param name="FacilityName">Operand A. Symbolic name of the Facility to seize.</param>
public sealed record SeizeBlock(GpssExpression FacilityName) : GpssBlock(), IKnownGpssBlock
{
    /// <inheritdoc/>
    public static string Keyword => typeof(SeizeBlock).DefaultGpssKeyword;
}
