namespace Gpss.Model.Blocks;

/// <summary>
/// Conventions for deriving GPSS metadata from <see cref="GpssBlock"/>-derived CLR types.
/// </summary>
public static class GpssBlockTypeExtensions
{
    extension(Type blockType)
    {
        /// <summary>
        /// The default GPSS keyword implied by <paramref name="blockType"/>'s name: the type name
        /// with a trailing <c>"Block"</c> suffix removed, upper-cased.
        /// E.g. <see cref="GenerateBlock"/> → <c>"GENERATE"</c>.
        /// </summary>
        /// <remarks>
        /// A convenience default for implementing <see cref="IKnownGpssBlock.Keyword"/> when the
        /// GPSS keyword matches the type name by convention. Not used automatically — types whose
        /// keyword doesn't follow this convention should return a literal string instead.
        /// </remarks>
        public string DefaultGpssKeyword =>
            (blockType.Name.EndsWith("Block", StringComparison.Ordinal)
                ? blockType.Name[..^"Block".Length]
                : blockType.Name).ToUpperInvariant();
    }
}
