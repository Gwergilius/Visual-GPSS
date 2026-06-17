using Gpss.Model.Variates;

namespace Gpss.Runtime;

/// <summary>
/// Default <see cref="IRandomNumberGeneratorFactory"/>. Pools one <see cref="IRandomNumberGenerator"/>
/// per stream number so that repeated requests for the same stream share its sequence, matching
/// the GPSS <c>RNn</c> convention. Use the seeded constructor for deterministic, reproducible runs.
/// </summary>
public sealed class SystemRandomNumberGeneratorFactory : IRandomNumberGeneratorFactory
{
    private readonly Dictionary<int, IRandomNumberGenerator> _streams = [];
    private readonly int? _seed;

    /// <summary>Initialises the factory with non-deterministic seeding for every stream.</summary>
    public SystemRandomNumberGeneratorFactory()
    {
    }

    /// <summary>Initialises the factory so each stream is seeded deterministically from <paramref name="seed"/>.</summary>
    /// <param name="seed">Base seed. Stream <c>n</c> is seeded with <c>seed + n - 1</c>.</param>
    public SystemRandomNumberGeneratorFactory(int seed) => _seed = seed;

    /// <inheritdoc/>
    public IRandomVariateGenerator Create(VariateSpec spec, int stream = 1) =>
        spec switch
        {
            UniformVariateSpec u => new UniformRandomVariateGenerator(GetOrCreateStream(stream), u.Mean, u.Spread),
            ConstantVariateSpec c => new ConstantRandomVariateGenerator(c.Value),
            _ => throw new NotSupportedException($"Variate spec '{spec.GetType().Name}' is not supported.")
        };

    private IRandomNumberGenerator GetOrCreateStream(int stream)
    {
        if (!_streams.TryGetValue(stream, out var generator))
        {
            generator = _seed is { } seed
                ? new SystemRandomNumberGenerator(seed + stream - 1)
                : new SystemRandomNumberGenerator();
            _streams[stream] = generator;
        }

        return generator;
    }
}
