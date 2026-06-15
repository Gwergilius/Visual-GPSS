using LexerNs = Gpss.Runtime.Lexer;
using TokenType = Gpss.Runtime.Lexer.TokenType;
using Xunit;

namespace Gpss.Runtime.Tests;

public sealed class LexerTests
{
    [Fact]
    public void Tokenize_EmptyString_ReturnsOnlyEof()
    {
        var lexer  = new LexerNs.Lexer("");
        var tokens = lexer.Tokenize();
        Assert.Single(tokens);
        Assert.Equal(TokenType.Eof, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_IntegerLiteral()
    {
        var tokens = new LexerNs.Lexer("42").Tokenize();
        Assert.Equal(TokenType.Integer, tokens[0].Type);
        Assert.Equal("42", tokens[0].Value);
    }

    [Fact]
    public void Tokenize_FloatLiteral()
    {
        var tokens = new LexerNs.Lexer("3.14").Tokenize();
        Assert.Equal(TokenType.Float, tokens[0].Type);
        Assert.Equal("3.14", tokens[0].Value);
    }

    [Theory]
    [InlineData("GENERATE",  TokenType.Generate)]
    [InlineData("TERMINATE", TokenType.Terminate)]
    [InlineData("SEIZE",     TokenType.Seize)]
    [InlineData("RELEASE",   TokenType.Release)]
    [InlineData("ADVANCE",   TokenType.Advance)]
    [InlineData("QUEUE",     TokenType.Queue)]
    [InlineData("DEPART",    TokenType.Depart)]
    [InlineData("START",     TokenType.Start)]
    [InlineData("ENTER",     TokenType.Enter)]
    [InlineData("LEAVE",     TokenType.Leave)]
    public void Tokenize_Keywords_AreRecognised(string keyword, TokenType expected)
    {
        var tokens = new LexerNs.Lexer(keyword).Tokenize();
        Assert.Equal(expected, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_StarAtColumn1_IsComment()
    {
        var tokens = new LexerNs.Lexer("* this is a comment\n").Tokenize();
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_SemiColon_IsComment()
    {
        var tokens = new LexerNs.Lexer("; another comment\n").Tokenize();
        Assert.Equal(TokenType.Comment, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_CommaAndOperands()
    {
        var tokens = new LexerNs.Lexer("100,5").Tokenize();
        Assert.Equal(TokenType.Integer, tokens[0].Type);
        Assert.Equal(TokenType.Comma,   tokens[1].Type);
        Assert.Equal(TokenType.Integer, tokens[2].Type);
    }

    [Fact]
    public void Tokenize_TrackLineNumbers()
    {
        var tokens = new LexerNs.Lexer("A\nB").Tokenize();
        Assert.Equal(1, tokens[0].Line); // A
        Assert.Equal(2, tokens[2].Line); // B (index 1 is Newline)
    }

    [Fact]
    public void Tokenize_CaseInsensitiveKeywords()
    {
        var lower = new LexerNs.Lexer("generate").Tokenize();
        var upper = new LexerNs.Lexer("GENERATE").Tokenize();
        Assert.Equal(TokenType.Generate, lower[0].Type);
        Assert.Equal(TokenType.Generate, upper[0].Type);
    }
}
