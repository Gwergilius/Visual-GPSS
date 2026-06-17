using Gpss.Model.Expressions;

namespace Gpss.Model.Variates;

/// <summary>
/// Base type for random-variate specifications. Captures which probability distribution a
/// GPSS operand pair (e.g. GENERATE/ADVANCE's mean and spread) describes, resolved once by
/// the parser so the runtime can request a matching generator without inspecting operand
/// shapes itself.
/// </summary>
public abstract record VariateSpec
{
    /// <summary>Creates a <see cref="ConstantVariateSpec"/> wrapping <paramref name="value"/>.</summary>
    /// <param name="value">The deterministic value.</param>
    public static VariateSpec Constant(GpssExpression value) => new ConstantVariateSpec(value);

    /// <summary>Creates a <see cref="UniformVariateSpec"/> from <paramref name="mean"/> and <paramref name="spread"/>.</summary>
    /// <param name="mean">Central value of the distribution.</param>
    /// <param name="spread">Half-range of variation around <paramref name="mean"/>.</param>
    public static VariateSpec Uniform(GpssExpression mean, GpssExpression spread) => new UniformVariateSpec(mean, spread);
}
