namespace Gpss.Runtime.Parser;

/// <summary>
/// Represents a single operand in a GPSS statement.
/// An operand can be a literal number, a label reference, an SNA
/// (Standard Numerical Attribute), or a compound expression.
/// </summary>
public abstract record Operand
{
    private Operand() { }

    /// <summary>An integer literal (e.g. <c>100</c>).</summary>
    public sealed record IntLiteral(int Value) : Operand;

    /// <summary>A floating-point literal (e.g. <c>3.14</c>).</summary>
    public sealed record FloatLiteral(double Value) : Operand;

    /// <summary>A symbolic label or entity name (e.g. <c>CASHIER</c>).</summary>
    public sealed record SymbolRef(string Name) : Operand;

    /// <summary>
    /// A Standard Numerical Attribute such as <c>Q$QUEUE1</c> or <c>FR$FAC1</c>.
    /// <para><c>Prefix</c> is the SNA type letter(s), e.g. "Q", "FR", "F".</para>
    /// </summary>
    public sealed record Sna(string Prefix, Operand Entity) : Operand;

    /// <summary>An arithmetic expression built from two operands.</summary>
    public sealed record BinaryExpr(Operand Left, char Op, Operand Right) : Operand;

    /// <summary>A function or variable reference with an argument.</summary>
    public sealed record FnRef(string Name, Operand Argument) : Operand;

    /// <summary>
    /// An empty (omitted) operand, used when a comma is present but the value is blank.
    /// </summary>
    public sealed record Empty : Operand;
}
