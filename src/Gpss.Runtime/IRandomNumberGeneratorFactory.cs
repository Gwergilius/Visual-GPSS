namespace Gpss.Runtime;

/// <summary>
/// Creates <see cref="IRandomVariateGenerator"/> instances bound to a specific random number
/// stream. Decouples block behaviours from a concrete generator instance, so that multiple
/// independent streams (matching the GPSS <c>RNn</c> convention) and alternative distributions
/// can be introduced later without changing the block behaviours that consume them.
/// </summary>
public interface IRandomNumberGeneratorFactory
{
    /// <summary>
    /// Returns a generator that samples uniformly distributed values from the given stream.
    /// Repeated calls with the same <paramref name="stream"/> draw from the same underlying
    /// sequence; different streams are independent of each other.
    /// </summary>
    /// <param name="stream">1-based random number stream identifier (GPSS <c>RNn</c> convention). Defaults to stream 1.</param>
    IRandomVariateGenerator CreateUniform(int stream = 1);
}
