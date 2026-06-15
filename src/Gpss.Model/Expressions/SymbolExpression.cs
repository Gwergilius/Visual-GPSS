namespace Gpss.Model.Expressions;

/// <summary>
/// A symbolic identifier used to reference named simulation entities such as
/// Facilities, Storages, and Queues.
/// </summary>
/// <param name="Name">The identifier as written in the GPSS source (case-insensitive in GPSS).</param>
public sealed record SymbolExpression(string Name) : GpssExpression;
