using Gpss.Model.Blocks;

namespace Gpss.Model;

/// <summary>
/// Represents a complete, parsed GPSS simulation model as an ordered sequence of blocks.
/// This is the root of the AST produced by the parser and consumed by the runtime.
/// </summary>
/// <param name="Blocks">
/// Ordered list of simulation blocks that form the model.
/// Blocks are executed in sequence; control flow follows transaction movement through them.
/// </param>
public sealed record GpssProgram(IReadOnlyList<GpssBlock> Blocks)
{
    /// <summary>
    /// Compares two programs by structural equality: same blocks in the same order.
    /// </summary>
    public bool Equals(GpssProgram? other) =>
        other is not null && Blocks.SequenceEqual(other.Blocks);

    /// <inheritdoc/>
    public override int GetHashCode() =>
        Blocks.Aggregate(0, (hash, block) => HashCode.Combine(hash, block));
}
