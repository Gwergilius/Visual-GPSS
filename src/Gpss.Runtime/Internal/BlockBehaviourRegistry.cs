using Gpss.Model.Blocks;

namespace Gpss.Runtime.Internal;

/// <summary>
/// Maps GPSS block types to their <see cref="IBlockBehaviour"/> implementations.
/// Populated via dependency injection; all registered <see cref="IBlockBehaviour"/> services
/// are collected into this registry at construction time.
/// </summary>
internal sealed class BlockBehaviourRegistry
{
    private readonly IReadOnlyDictionary<Type, IBlockBehaviour> _behaviours;

    /// <summary>
    /// Initialises the registry from all registered <see cref="IBlockBehaviour"/> instances.
    /// </summary>
    /// <param name="behaviours">All behaviour implementations provided by DI.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="KnownGpssBlocks"/> lists a block type for which no
    /// <see cref="IBlockBehaviour"/> was supplied, i.e. the parser and runtime have drifted out of sync.
    /// </exception>
    internal BlockBehaviourRegistry(IEnumerable<IBlockBehaviour> behaviours)
    {
        _behaviours = behaviours.ToDictionary(b => b.BlockType);

        var missing = KnownGpssBlocks.ByName.Values.Where(t => !_behaviours.ContainsKey(t)).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException(
                $"No behaviour registered for known block type(s): {string.Join(", ", missing.Select(t => t.Name))}. " +
                "Every block listed in KnownGpssBlocks must have a corresponding IBlockBehaviour implementation.");
    }

    /// <summary>
    /// Returns the behaviour registered for the type of <paramref name="block"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// Thrown when no behaviour is registered for the block's type.
    /// </exception>
    internal IBlockBehaviour For(GpssBlock block) =>
        _behaviours.TryGetValue(block.GetType(), out var behaviour)
            ? behaviour
            : throw new NotSupportedException(
                $"No behaviour registered for block type '{block.GetType().Name}'. " +
                "Register a corresponding IBlockBehaviour implementation in the DI container.");
}
