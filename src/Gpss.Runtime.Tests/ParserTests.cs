using LexerNs = Gpss.Runtime.Lexer;
using ParserNs = Gpss.Runtime.Parser;
using TokenType = Gpss.Runtime.Lexer.TokenType;
using Xunit;

namespace Gpss.Runtime.Tests;

public sealed class ParserTests
{
    private static IReadOnlyList<ParserNs.Statement> Parse(string src)
    {
        var tokens = new LexerNs.Lexer(src).Tokenize();
        return new ParserNs.Parser(tokens).Parse();
    }

    [Fact]
    public void Parse_EmptySource_ReturnsNoStatements()
    {
        var stmts = Parse("");
        Assert.Empty(stmts);
    }

    [Fact]
    public void Parse_Generate_WithTwoOperands()
    {
        var stmts = Parse("GENERATE 100,5");
        Assert.Single(stmts);
        var s = stmts[0];
        Assert.Equal(TokenType.Generate, s.OperationType);
        Assert.Equal(2, s.Operands.Count);
        Assert.IsType<ParserNs.Operand.IntLiteral>(s.Operands[0]);
        Assert.Equal(100, ((ParserNs.Operand.IntLiteral)s.Operands[0]).Value);
        Assert.Equal(5,   ((ParserNs.Operand.IntLiteral)s.Operands[1]).Value);
    }

    [Fact]
    public void Parse_Terminate_WithLabel()
    {
        var stmts = Parse("DONE TERMINATE 1");
        Assert.Single(stmts);
        Assert.Equal("DONE", stmts[0].Label);
        Assert.Equal(TokenType.Terminate, stmts[0].OperationType);
    }

    [Fact]
    public void Parse_MultipleStatements()
    {
        var src = """
            GENERATE 10,2
            SEIZE    CASHIER
            ADVANCE  5
            RELEASE  CASHIER
            TERMINATE 1
            """;
        var stmts = Parse(src);
        Assert.Equal(5, stmts.Count);
        Assert.Equal(TokenType.Generate,  stmts[0].OperationType);
        Assert.Equal(TokenType.Seize,     stmts[1].OperationType);
        Assert.Equal(TokenType.Advance,   stmts[2].OperationType);
        Assert.Equal(TokenType.Release,   stmts[3].OperationType);
        Assert.Equal(TokenType.Terminate, stmts[4].OperationType);
    }

    [Fact]
    public void Parse_CommentLinesAreSkipped()
    {
        var src = """
            * This is a comment
            GENERATE 10
            ; Also a comment
            TERMINATE 1
            """;
        var stmts = Parse(src);
        Assert.Equal(2, stmts.Count);
    }

    [Fact]
    public void Parse_EmptyOperandInMiddle()
    {
        // GENERATE 10,,3  →  operands: 10, Empty, 3
        var stmts = Parse("GENERATE 10,,3");
        Assert.Single(stmts);
        Assert.Equal(3, stmts[0].Operands.Count);
        Assert.IsType<ParserNs.Operand.IntLiteral>(stmts[0].Operands[0]);
        Assert.IsType<ParserNs.Operand.Empty>(stmts[0].Operands[1]);
        Assert.IsType<ParserNs.Operand.IntLiteral>(stmts[0].Operands[2]);
    }

    [Fact]
    public void Parse_SymbolReference()
    {
        var stmts = Parse("SEIZE CASHIER");
        Assert.Single(stmts);
        Assert.IsType<ParserNs.Operand.SymbolRef>(stmts[0].Operands[0]);
        Assert.Equal("CASHIER", ((ParserNs.Operand.SymbolRef)stmts[0].Operands[0]).Name);
    }

    [Fact]
    public void Parse_StatementGetReturnsEmptyForMissingIndex()
    {
        var stmts = Parse("ADVANCE 5");
        Assert.IsType<ParserNs.Operand.Empty>(stmts[0].Get(5));
    }
}
