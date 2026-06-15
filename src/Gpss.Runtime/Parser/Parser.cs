using Gpss.Runtime.Lexer;

namespace Gpss.Runtime.Parser;

/// <summary>
/// Parses a stream of GPSS tokens into a list of <see cref="Statement"/> objects.
/// </summary>
public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _pos;

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    private Token Current => _pos < _tokens.Count ? _tokens[_pos] : _tokens[^1];
    private Token Peek(int offset = 1) =>
        (_pos + offset) < _tokens.Count ? _tokens[_pos + offset] : _tokens[^1];

    private Token Consume()
    {
        var t = Current;
        _pos++;
        return t;
    }

    private void SkipNewlines()
    {
        while (Current.Type is TokenType.Newline or TokenType.Comment)
            Consume();
    }

    /// <summary>Parses all statements from the token stream.</summary>
    public IReadOnlyList<Statement> Parse()
    {
        var statements = new List<Statement>();
        SkipNewlines();
        while (Current.Type != TokenType.Eof)
        {
            var stmt = ParseStatement();
            if (stmt != null)
                statements.Add(stmt);
            SkipNewlines();
        }
        return statements;
    }

    private Statement? ParseStatement()
    {
        // A statement can optionally begin with a label (identifier followed by a
        // keyword or another identifier that is the operation).
        string? label = null;
        int statementLine = Current.Line;

        Token first = Current;

        // Determine if first token is a label or the operation.
        if (first.Type == TokenType.Identifier)
        {
            // Look ahead: if the next non-space token is also an identifier or keyword
            // then first is the label.
            Token next = Peek();
            if (IsOperationToken(next))
            {
                label = first.Value;
                Consume(); // consume label
            }
        }

        // Now read the operation
        Token opToken = Current;
        if (!IsOperationToken(opToken))
        {
            // Skip unknown / unexpected tokens
            Consume();
            return null;
        }
        Consume(); // consume operation

        // Read operands (comma-separated, until end of line)
        var operands = ParseOperandList();

        // Skip trailing comment on same line
        if (Current.Type == TokenType.Comment)
            Consume();

        return new Statement
        {
            Label         = label,
            Operation     = opToken.Value,
            OperationType = opToken.Type,
            Operands      = operands,
            Line          = statementLine,
        };
    }

    private static bool IsOperationToken(Token t) =>
        t.Type != TokenType.Newline &&
        t.Type != TokenType.Eof &&
        t.Type != TokenType.Comment &&
        t.Type != TokenType.Comma &&
        t.Type != TokenType.Unknown;

    private List<Operand> ParseOperandList()
    {
        var operands = new List<Operand>();
        // No operands if we're at end of line
        if (Current.Type is TokenType.Newline or TokenType.Eof or TokenType.Comment)
            return operands;

        operands.Add(ParseOperand());

        while (Current.Type == TokenType.Comma)
        {
            Consume(); // consume comma
            operands.Add(ParseOperand());
        }
        return operands;
    }

    private Operand ParseOperand()
    {
        // Empty operand (two consecutive commas)
        if (Current.Type is TokenType.Comma or TokenType.Newline or TokenType.Eof)
            return new Operand.Empty();

        return ParseAdditive();
    }

    private Operand ParseAdditive()
    {
        var left = ParseMultiplicative();
        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            char op = Current.Value[0];
            Consume();
            var right = ParseMultiplicative();
            left = new Operand.BinaryExpr(left, op, right);
        }
        return left;
    }

    private Operand ParseMultiplicative()
    {
        var left = ParseUnary();
        while (Current.Type is TokenType.Star or TokenType.Slash)
        {
            char op = Current.Value[0];
            Consume();
            var right = ParseUnary();
            left = new Operand.BinaryExpr(left, op, right);
        }
        return left;
    }

    private Operand ParseUnary()
    {
        if (Current.Type == TokenType.Minus)
        {
            Consume();
            var operand = ParsePrimary();
            return new Operand.BinaryExpr(new Operand.IntLiteral(0), '-', operand);
        }
        return ParsePrimary();
    }

    private Operand ParsePrimary()
    {
        var tok = Current;

        if (tok.Type == TokenType.Integer)
        {
            Consume();
            return new Operand.IntLiteral(int.Parse(tok.Value));
        }

        if (tok.Type == TokenType.Float)
        {
            Consume();
            return new Operand.FloatLiteral(double.Parse(tok.Value,
                System.Globalization.CultureInfo.InvariantCulture));
        }

        if (tok.Type == TokenType.OpenParen)
        {
            Consume();
            var inner = ParseAdditive();
            if (Current.Type == TokenType.CloseParen) Consume();
            return inner;
        }

        if (tok.Type == TokenType.String)
        {
            Consume();
            return new Operand.SymbolRef(tok.Value);
        }

        // Identifier – could be a symbol, SNA, or function call
        if (tok.Type == TokenType.Identifier || IsKeyword(tok.Type))
        {
            Consume();
            string name = tok.Value;

            // SNA: identifier followed by $ (e.g. Q$QUEUE1, FR$FAC)
            if (Current.Type == TokenType.Dollar)
            {
                Consume();
                var entity = ParsePrimary();
                return new Operand.Sna(name, entity);
            }

            // Function application: NAME(arg)
            if (Current.Type == TokenType.OpenParen)
            {
                Consume();
                var arg = ParseAdditive();
                if (Current.Type == TokenType.CloseParen) Consume();
                return new Operand.FnRef(name, arg);
            }

            return new Operand.SymbolRef(name);
        }

        // Fallback: skip and return empty
        Consume();
        return new Operand.Empty();
    }

    private static bool IsKeyword(TokenType t) =>
        t != TokenType.Newline &&
        t != TokenType.Eof &&
        t != TokenType.Comment &&
        t != TokenType.Unknown &&
        t != TokenType.Integer &&
        t != TokenType.Float &&
        t != TokenType.String &&
        t != TokenType.Identifier &&
        t != TokenType.Comma &&
        t != TokenType.Plus &&
        t != TokenType.Minus &&
        t != TokenType.Star &&
        t != TokenType.Slash &&
        t != TokenType.Hash &&
        t != TokenType.At &&
        t != TokenType.Dollar &&
        t != TokenType.Ampersand &&
        t != TokenType.Percent &&
        t != TokenType.Caret &&
        t != TokenType.OpenParen &&
        t != TokenType.CloseParen &&
        t != TokenType.Colon &&
        t != TokenType.SemiColon;
}
