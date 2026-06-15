using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

public sealed record GenerateBlock(
    string? Label,
    GpssExpression MeanInterArrivalTime,
    GpssExpression? Spread = null,
    GpssExpression? FirstTransactionOffset = null,
    GpssExpression? GenerationLimit = null,
    GpssExpression? Priority = null
) : GpssBlock(Label);
