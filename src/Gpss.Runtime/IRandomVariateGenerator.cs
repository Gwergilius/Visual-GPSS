namespace Gpss.Runtime;

/// <summary>
/// Produces a numeric value sampled from a specific probability distribution. Each instance is
/// bound to the operand expressions it was created for (e.g. GENERATE or ADVANCE's mean and
/// spread), evaluating them afresh on every <see cref="Sample"/> call so dynamic GPSS
/// expressions stay current.
/// </summary>
public interface IRandomVariateGenerator
{
    /// <summary>Samples the next value from this generator's distribution.</summary>
    double Sample();
}
