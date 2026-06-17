namespace Gpss.Model.Blocks;

/// <summary>
/// Marks a <see cref="GpssBlock"/> type as recognised by the toolchain and requires it to declare
/// its GPSS source keyword.
/// </summary>
/// <remarks>
/// Implement <see cref="Keyword"/> with a plain <c>public static</c> property (not an explicit
/// interface implementation), so that <see cref="KnownGpssBlocks"/> can read it via reflection.
/// Use <c>typeof(TBlock).DefaultGpssKeyword</c> (see <see cref="GpssBlockTypeExtensions"/>) to
/// derive the keyword from the type name by convention, or return a literal string when the
/// keyword doesn't follow that convention.
/// </remarks>
public interface IKnownGpssBlock
{
    /// <summary>The GPSS source keyword for this block, e.g. <c>"GENERATE"</c>.</summary>
    static abstract string Keyword { get; }
}
