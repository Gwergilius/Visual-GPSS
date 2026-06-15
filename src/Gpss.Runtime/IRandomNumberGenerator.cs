namespace Gpss.Runtime;

/// <summary>
/// Provides random number generation for stochastic GPSS operands (e.g. GENERATE spread).
/// Implementations can wrap <see cref="System.Random"/>, a cryptographic RNG, or a
/// deterministic sequence for testing.
/// </summary>
public interface IRandomNumberGenerator
{
    /// <summary>Returns a random <see cref="double"/> in the range [0.0, 1.0).</summary>
    double NextDouble();

    /// <summary>Returns a uniformly distributed <see cref="double"/> in the range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound.</param>
    double NextUniform(double min, double max);

    /// <summary>Returns a uniformly distributed <see cref="int"/> in the range [<paramref name="min"/>, <paramref name="max"/>].</summary>
    /// <param name="min">Inclusive lower bound.</param>
    /// <param name="max">Inclusive upper bound.</param>
    int NextInt(int min, int max);
}
