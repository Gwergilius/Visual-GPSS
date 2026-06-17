namespace Gpss.Model.Blocks;

/// <summary>Base type for all GPSS simulation blocks.</summary>
/// <param name="Label">Optional symbolic name used to reference this block from other blocks. Null when the block is unnamed.</param>
/// <param name="Description">
/// Human-readable description of this block, or <see langword="null"/> when it has none.
/// Carried through to runtime log messages so the author's explanation of intent stays visible
/// when watching or diagnosing a simulation run. Populated by the parser from the source line's
/// trailing comment; the model itself has no notion of comments.
/// </param>
public abstract record GpssBlock(string? Label = null, string? Description = null);
