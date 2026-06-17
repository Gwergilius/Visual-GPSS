using System.Reflection;

namespace Gpss.Model.Blocks;

/// <summary>
/// Canonical registry of GPSS block keywords recognised by the toolchain, mapped to the
/// <see cref="GpssBlock"/>-derived type that represents them.
/// </summary>
/// <remarks>
/// Built lazily, on first access, by reflecting over every concrete <see cref="IKnownGpssBlock"/>
/// implementation in this assembly. Both <c>Gpss.Parser</c> (to validate block name tokens) and
/// <c>Gpss.Runtime</c> (to verify that every recognised block has a corresponding
/// <c>IBlockBehaviour</c> implementation) consult this single list, so the two cannot drift out of
/// sync, and a new block becomes "known" as soon as it implements <see cref="IKnownGpssBlock"/> —
/// no separate registration list to maintain.
/// </remarks>
public static class KnownGpssBlocks
{
    private static readonly Lazy<IReadOnlyDictionary<string, Type>> LazyByName = new(Discover);

    /// <summary>Maps each recognised GPSS block keyword (case-insensitive) to its model type.</summary>
    public static IReadOnlyDictionary<string, Type> ByName => LazyByName.Value;

    /// <summary>Returns whether <paramref name="name"/> is a recognised GPSS block keyword.</summary>
    /// <param name="name">The candidate block keyword, compared case-insensitively.</param>
    public static bool IsKnown(string name) => ByName.ContainsKey(name);

    private static IReadOnlyDictionary<string, Type> Discover() =>
        typeof(IKnownGpssBlock).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } && t.IsAssignableTo(typeof(IKnownGpssBlock)))
            .ToDictionary(KeywordOf, static t => t, StringComparer.OrdinalIgnoreCase);

    private static string KeywordOf(Type blockType) =>
        (string)blockType
            .GetProperty(nameof(IKnownGpssBlock.Keyword), BindingFlags.Public | BindingFlags.Static)!
            .GetValue(null)!;
}
