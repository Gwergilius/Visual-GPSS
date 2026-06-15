namespace Gpss.Runtime.Lexer;

/// <summary>A single token produced by the GPSS lexer.</summary>
public sealed record Token(
    TokenType Type,
    string Value,
    int Line,
    int Column);
