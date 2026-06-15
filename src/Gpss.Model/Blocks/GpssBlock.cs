namespace Gpss.Model.Blocks;

/// <summary>Base type for all GPSS simulation blocks.</summary>
/// <param name="Label">Optional symbolic name used to reference this block from other blocks. Null when the block is unnamed.</param>
public abstract record GpssBlock(string? Label = null);
