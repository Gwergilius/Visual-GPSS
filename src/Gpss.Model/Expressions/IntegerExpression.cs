namespace Gpss.Model.Expressions;

/// <summary>A literal integer operand value.</summary>
/// <param name="Value">The integer value of the operand.</param>
public sealed record IntegerExpression(int Value) : GpssExpression;
