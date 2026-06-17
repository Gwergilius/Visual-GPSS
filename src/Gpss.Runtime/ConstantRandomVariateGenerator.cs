using Gpss.Model.Expressions;
using static Gpss.Runtime.Internal.ExpressionEvaluator;

namespace Gpss.Runtime;

/// <summary>
/// Samples a fixed, deterministic value without consuming any random number stream.
/// </summary>
/// <param name="value">Expression yielding the deterministic value, evaluated afresh on every <see cref="Sample"/> call.</param>
public sealed class ConstantRandomVariateGenerator(GpssExpression value) : IRandomVariateGenerator
{
    /// <inheritdoc/>
    public double Sample() => Evaluate(value);
}
