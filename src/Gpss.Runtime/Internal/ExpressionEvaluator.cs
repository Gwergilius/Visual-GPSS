using Gpss.Model.Expressions;

namespace Gpss.Runtime.Internal;

/// <summary>Evaluates GPSS operand expressions to their runtime values.</summary>
internal static class ExpressionEvaluator
{
    /// <summary>
    /// Evaluates <paramref name="expression"/> to a <see cref="double"/> numeric value.
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

    /// <summary>
    /// Evaluates <paramref name="expression"/> to its symbolic name string.
    /// Used for entity identifiers such as Facility and Queue names.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Thrown when the expression type cannot yield a name.
    /// </exception>
    internal static string EvaluateName(GpssExpression expression) =>
        expression switch
        {
            SymbolExpression s => s.Name,
            _ => throw new NotSupportedException(
                $"Expression type '{expression.GetType().Name}' cannot be evaluated as a name.")
        };
}
