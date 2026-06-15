using Gpss.Model.Expressions;

namespace Gpss.Model.Blocks;

public sealed record TerminateBlock(
    string? Label,
    GpssExpression? DecrementCount = null
) : GpssBlock(Label);
