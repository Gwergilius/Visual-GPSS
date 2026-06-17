using Gpss.Model.Expressions;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime;

/// <summary>
/// Samples values uniformly distributed in the inclusive integer range
/// [mean − spread, mean + spread], matching the GPSS convention for operand pairs
/// such as GENERATE/ADVANCE's A (mean) and B (spread). <paramref name="mean"/> and
/// <paramref name="spread"/> are evaluated afresh on every <see cref="Sample"/> call.
/// </summary>
/// <param name="source">The underlying random number stream to draw from.</param>
/// <param name="mean">Expression yielding the central value of the distribution.</param>
/// <param name="spread">Expression yielding the half-range of variation around the mean.</param>
public sealed class UniformRandomVariateGenerator(IRandomNumberGenerator source, GpssExpression mean, GpssExpression spread)
    : IRandomVariateGenerator
{
    /// <inheritdoc/>
    public double Sample()
    {
        var m = Evaluate(mean);
        var s = Evaluate(spread);
        return s == 0
            ? m
            : source.NextInt((int)Math.Round(m - s), (int)Math.Round(m + s));
    }
}
