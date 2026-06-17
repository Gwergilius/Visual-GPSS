using Gpss.Model.Expressions;

namespace Gpss.Model.Variates;

/// <summary>
/// Specifies a fixed, deterministic value with no random variation. Equivalent to a
/// <see cref="UniformVariateSpec"/> with zero spread, but lets the runtime skip drawing
/// from the random number stream entirely.
/// </summary>
/// <param name="Value">The deterministic value.</param>
public sealed record ConstantVariateSpec(GpssExpression Value) : VariateSpec;
