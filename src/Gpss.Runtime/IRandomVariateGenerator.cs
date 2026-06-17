namespace Gpss.Runtime;

/// <summary>
/// Produces a numeric value sampled from a specific probability distribution, derived from
/// a GPSS-style mean/spread operand pair (e.g. GENERATE or ADVANCE operands A and B).
/// </summary>
public interface IRandomVariateGenerator
{
    /// <summary>
    /// Samples a value based on <paramref name="mean"/> and <paramref name="spread"/>.
    /// The precise relationship between these inputs and the result depends on the
    /// implementing distribution; a <paramref name="spread"/> of zero always yields <paramref name="mean"/>.
    /// </summary>
    /// <param name="mean">Central value of the distribution.</param>
    /// <param name="spread">Dispersion parameter (e.g. half-range for a uniform distribution). Zero means no variation.</param>
    double Sample(double mean, double spread);
}
