using Gpss.Model.Blocks;

namespace Gpss.Model;

public sealed record GpssProgram(IReadOnlyList<GpssBlock> Blocks)
{
    public bool Equals(GpssProgram? other) =>
        other is not null && Blocks.SequenceEqual(other.Blocks);

    public override int GetHashCode() =>
        Blocks.Aggregate(0, (hash, block) => HashCode.Combine(hash, block));
}
