using Gpss.Model.Expressions;

namespace Gpss.Model.Variates;

/// <summary>
/// Specifies a value sampled uniformly from [Mean − Spread, Mean + Spread].
/// </summary>
/// <param name="Mean">Central value of the distribution.</param>
/// <param name="Spread">Half-range of variation around <paramref name="Mean"/>.</param>
public sealed record UniformVariateSpec(GpssExpression Mean, GpssExpression Spread) : VariateSpec;
