using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

/// <summary>
/// Relinquishes exclusive ownership of a Facility previously obtained with SEIZE.
/// If transactions are waiting for the Facility, ownership is immediately transferred
/// to the next one.
/// Corresponds to the GPSS RELEASE block (operand A: facility name).
/// </summary>
/// <param name="FacilityName">Operand A. Symbolic name of the Facility to release.</param>
public sealed record ReleaseBlock(GpssExpression FacilityName) : GpssBlock();
