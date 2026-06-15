namespace Gpss.Runtime.Lexer;

/// <summary>Kinds of tokens produced by the GPSS lexer.</summary>
public enum TokenType
{
    // Literals
    Integer,
    Float,
    String,
    Identifier,

    // Block names
    Generate,
    Terminate,
    Seize,
    Release,
    Enter,
    Leave,
    Queue,
    Depart,
    Advance,
    Transfer,
    Test,
    Assign,
    Tabulate,
    Logic,
    Gate,
    Priority,
    Mark,
    Savevalue,
    Msavevalue,
    Loop,
    Index,
    Count,
    Select,
    Split,
    Assemble,
    Match,
    Gather,
    Preempt,
    Return,

    // Control statements
    Start,
    End,
    Reset,
    Clear,
    Simulate,

    // Definitions
    Storage,
    Table,
    Qtable,
    Variable,
    Fvariable,
    Bvariable,
    Matrix,
    Initial,
    Rmult,
    Seed,
    Equ,
    Function,

    // Operators / punctuation
    Comma,
    Plus,
    Minus,
    Star,
    Slash,
    Hash,
    At,
    Dollar,
    Ampersand,
    Percent,
    Caret,
    OpenParen,
    CloseParen,
    Colon,
    SemiColon,

    // Special
    Newline,
    Comment,
    Eof,
    Unknown,
}
