using Gpss.Model.Blocks;

namespace Gpss.Parser.Internal;

/// <summary>
/// Maps GPSS block keywords to their <see cref="IBlockBuilder"/> implementations.
/// Populated via dependency injection; all registered <see cref="IBlockBuilder"/> services
/// are collected into this registry at construction time.
/// </summary>
internal sealed class BlockBuilderRegistry
{
    private readonly IReadOnlyDictionary<string, IBlockBuilder> _byKeyword;

    /// <summary>
    /// Initialises the registry from all registered <see cref="IBlockBuilder"/> instances.
    /// </summary>
    /// <param name="builders">All builder implementations provided by DI.</param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when <see cref="KnownGpssBlocks"/> lists a block type for which no
    /// <see cref="IBlockBuilder"/> was supplied, i.e. the block registry and the parser have drifted out of sync.
    /// </exception>
    internal BlockBuilderRegistry(IEnumerable<IBlockBuilder> builders)
    {
        var byType = builders.ToDictionary(b => b.BlockType);

        var missing = KnownGpssBlocks.ByName.Values.Where(t => !byType.ContainsKey(t)).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException(
                $"No builder registered for known block type(s): {string.Join(", ", missing.Select(t => t.Name))}. " +
                "Every block listed in KnownGpssBlocks must have a corresponding IBlockBuilder implementation.");

        _byKeyword = KnownGpssBlocks.ByName.ToDictionary(
            kv => kv.Key, kv => byType[kv.Value], StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns the builder registered for <paramref name="keyword"/>.
    /// </summary>
    /// <param name="keyword">A GPSS block keyword (case-insensitive), e.g. <c>"GENERATE"</c>.</param>
    /// <exception cref="NotSupportedException">
    /// Thrown when no builder is registered for <paramref name="keyword"/>.
    /// </exception>
    internal IBlockBuilder For(string keyword) =>
        _byKeyword.TryGetValue(keyword, out var builder)
            ? builder
            : throw new NotSupportedException(
                $"No builder registered for block keyword '{keyword}'. " +
                "Register a corresponding IBlockBuilder implementation in the DI container.");
}
