namespace Gpss.Runtime;

/// <summary>
/// Default <see cref="IRandomNumberGenerator"/> implementation backed by <see cref="System.Random"/>.
/// Use the seeded constructor for deterministic, reproducible simulation runs.
/// </summary>
public sealed class SystemRandomNumberGenerator : IRandomNumberGenerator
{
    private readonly Random _random;

    /// <summary>Initialises the generator with a non-deterministic seed.</summary>
    public SystemRandomNumberGenerator() => _random = new Random();

    /// <summary>Initialises the generator with a fixed <paramref name="seed"/> for reproducible runs.</summary>
    /// <param name="seed">The seed value.</param>
    public SystemRandomNumberGenerator(int seed) => _random = new Random(seed);

    /// <inheritdoc/>
    public double NextDouble() => _random.NextDouble();

    /// <inheritdoc/>
    public double NextUniform(double min, double max) => min + _random.NextDouble() * (max - min);

    /// <inheritdoc/>
    public int NextInt(int min, int max) => _random.Next(min, max + 1);
}
