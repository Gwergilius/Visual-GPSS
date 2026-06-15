using Gpss.Runtime.Lexer;

namespace Gpss.Runtime.Parser;

/// <summary>
/// Represents a single GPSS statement (one line) after parsing.
/// </summary>
public sealed class Statement
{
    /// <summary>Optional label at column 1 (e.g. <c>LOOP1</c>).</summary>
    public string? Label { get; init; }

    /// <summary>The operation keyword (e.g. <c>GENERATE</c>).</summary>
    public required string Operation { get; init; }

    /// <summary>The raw keyword token type.</summary>
    public required TokenType OperationType { get; init; }

    /// <summary>The operand list (A, B, C, …).</summary>
    public required IReadOnlyList<Operand> Operands { get; init; }

    /// <summary>Source line number (1-based).</summary>
    public int Line { get; init; }

    /// <summary>Returns operand at position <paramref name="index"/> (0-based), or <see cref="Operand.Empty"/> if absent.</summary>
    public Operand Get(int index) =>
        index < Operands.Count ? Operands[index] : new Operand.Empty();
}
