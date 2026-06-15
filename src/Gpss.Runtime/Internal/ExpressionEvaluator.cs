using Gpss.Model.Expressions;

namespace Gpss.Runtime.Internal;

/// <summary>Evaluates GPSS operand expressions to their runtime double values.</summary>
internal static class ExpressionEvaluator
{
    /// <summary>
    /// Evaluates <paramref name="expression"/> to a <see cref="double"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Thrown when the expression type has no runtime implementation yet.
    /// </exception>
    internal static double Evaluate(GpssExpression expression) =>
        expression switch
        {
            IntegerExpression i => (double)i.Value,
            _ => throw new NotSupportedException(
                $"Expression type '{expression.GetType().Name}' is not supported by the runtime.")
        };
}
