using Gpss.Model.Variates;

namespace Gpss.Runtime;

/// <summary>
/// Creates <see cref="IRandomVariateGenerator"/> instances matching a <see cref="VariateSpec"/>,
/// bound to a specific random number stream. The parser/compiler decides which distribution a
/// block uses by producing the appropriate <see cref="VariateSpec"/>; this factory resolves that
/// decision to a concrete generator so block behaviours never need to branch on distribution kind.
/// </summary>
public interface IRandomNumberGeneratorFactory
{
    /// <summary>
    /// Returns a generator matching <paramref name="spec"/>, drawing from the given
    /// <paramref name="stream"/> when the distribution requires randomness. Repeated calls with
    /// the same <paramref name="stream"/> draw from the same underlying sequence; different
    /// streams are independent of each other.
    /// </summary>
    /// <param name="spec">Distribution and parameters to sample from.</param>
    /// <param name="stream">1-based random number stream identifier (GPSS <c>RNn</c> convention). Defaults to stream 1.</param>
    IRandomVariateGenerator Create(VariateSpec spec, int stream = 1);
}
