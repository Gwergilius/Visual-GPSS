namespace Gpss.Runtime;

/// <summary>
/// Samples values uniformly distributed in the inclusive integer range
/// [mean − spread, mean + spread], matching the GPSS convention for operand pairs
/// such as GENERATE/ADVANCE's A (mean) and B (spread).
/// </summary>
/// <param name="source">The underlying random number stream to draw from.</param>
public sealed class UniformRandomVariateGenerator(IRandomNumberGenerator source) : IRandomVariateGenerator
{
    /// <inheritdoc/>
    public double Sample(double mean, double spread) =>
        spread == 0
            ? mean
            : source.NextInt((int)Math.Round(mean - spread), (int)Math.Round(mean + spread));
}
