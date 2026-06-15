namespace Gpss.Runtime.Simulation;

/// <summary>
/// Random number generator used by GPSS distributions.
/// GPSS classically used a multiplicative congruential generator;
/// this implementation wraps .NET's <see cref="Random"/> while
/// preserving the same interface.
/// </summary>
public sealed class GpssRng
{
    private readonly Random _rng;

    public GpssRng(int seed = 0) =>
        _rng = seed == 0 ? Random.Shared : new Random(seed);

    /// <summary>Returns a uniform random number in [0, 1).</summary>
    public double Uniform() => _rng.NextDouble();

    /// <summary>
    /// Exponential distribution with mean <paramref name="mean"/>.
    /// Used for inter-arrival and service times.
    /// </summary>
    public double Exponential(double mean)
    {
        if (mean <= 0) return 0;
        return -mean * Math.Log(1.0 - _rng.NextDouble());
    }

    /// <summary>
    /// Normal distribution with given mean and standard deviation
    /// (Box–Muller transform).
    /// </summary>
    public double Normal(double mean, double stdDev)
    {
        double u1 = 1.0 - _rng.NextDouble();
        double u2 = 1.0 - _rng.NextDouble();
        double z  = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
        return mean + stdDev * z;
    }

    /// <summary>Uniform integer in [min, max] (inclusive).</summary>
    public int UniformInt(int min, int max) => _rng.Next(min, max + 1);

    /// <summary>
    /// GPSS-style random variate: mean ± modifier (uniform spread).
    /// GENERATE A,B  →  uniform(A-B, A+B) when modifier is specified.
    /// </summary>
    public double UniformSpread(double mean, double halfRange)
    {
        if (halfRange <= 0) return mean;
        return mean - halfRange + 2 * halfRange * _rng.NextDouble();
    }
}
