using System.Collections.Frozen;

namespace Gpss.Runtime.Lexer;

/// <summary>
/// Converts a GPSS source string into a sequence of <see cref="Token"/> objects.
/// </summary>
/// <remarks>
/// GPSS source format (fixed-field):
///   columns  1-5  : optional label
///   columns  7-14 : operation (block name or directive)
///   columns  15+  : operands (comma-separated)
///   semicolon or * at column 1 starts a comment line
/// The lexer is line-oriented but also works on free-form text.
/// </remarks>
public sealed class Lexer
{
    private static readonly FrozenDictionary<string, TokenType> Keywords =
        new Dictionary<string, TokenType>(StringComparer.OrdinalIgnoreCase)
        {
            ["GENERATE"]  = TokenType.Generate,
            ["TERMINATE"] = TokenType.Terminate,
            ["SEIZE"]     = TokenType.Seize,
            ["RELEASE"]   = TokenType.Release,
            ["ENTER"]     = TokenType.Enter,
            ["LEAVE"]     = TokenType.Leave,
            ["QUEUE"]     = TokenType.Queue,
            ["DEPART"]    = TokenType.Depart,
            ["ADVANCE"]   = TokenType.Advance,
            ["TRANSFER"]  = TokenType.Transfer,
            ["TEST"]      = TokenType.Test,
            ["ASSIGN"]    = TokenType.Assign,
            ["TABULATE"]  = TokenType.Tabulate,
            ["LOGIC"]     = TokenType.Logic,
            ["GATE"]      = TokenType.Gate,
            ["PRIORITY"]  = TokenType.Priority,
            ["MARK"]      = TokenType.Mark,
            ["SAVEVALUE"] = TokenType.Savevalue,
            ["MSAVEVALUE"]= TokenType.Msavevalue,
            ["LOOP"]      = TokenType.Loop,
            ["INDEX"]     = TokenType.Index,
            ["COUNT"]     = TokenType.Count,
            ["SELECT"]    = TokenType.Select,
            ["SPLIT"]     = TokenType.Split,
            ["ASSEMBLE"]  = TokenType.Assemble,
            ["MATCH"]     = TokenType.Match,
            ["GATHER"]    = TokenType.Gather,
            ["PREEMPT"]   = TokenType.Preempt,
            ["RETURN"]    = TokenType.Return,

            ["START"]    = TokenType.Start,
            ["END"]      = TokenType.End,
            ["RESET"]    = TokenType.Reset,
            ["CLEAR"]    = TokenType.Clear,
            ["SIMULATE"] = TokenType.Simulate,

            ["STORAGE"]   = TokenType.Storage,
            ["TABLE"]     = TokenType.Table,
            ["QTABLE"]    = TokenType.Qtable,
            ["VARIABLE"]  = TokenType.Variable,
            ["FVARIABLE"] = TokenType.Fvariable,
            ["BVARIABLE"] = TokenType.Bvariable,
            ["MATRIX"]    = TokenType.Matrix,
            ["INITIAL"]   = TokenType.Initial,
            ["RMULT"]     = TokenType.Rmult,
            ["SEED"]      = TokenType.Seed,
            ["EQU"]       = TokenType.Equ,
            ["FUNCTION"]  = TokenType.Function,
        }.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);

    private readonly string _source;
    private int _pos;
    private int _line;
    private int _col;

    public Lexer(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _pos  = 0;
        _line = 1;
        _col  = 1;
    }

    /// <summary>Tokenizes the entire source and returns all tokens.</summary>
    public IReadOnlyList<Token> Tokenize()
    {
        var tokens = new List<Token>();
        while (true)
        {
            var token = Next();
            tokens.Add(token);
            if (token.Type == TokenType.Eof) break;
        }
        return tokens;
    }

    private Token Next()
    {
        // Skip spaces (but not newlines)
        while (_pos < _source.Length && _source[_pos] == ' ')
            Advance();

        if (_pos >= _source.Length)
            return Make(TokenType.Eof, string.Empty);

        char c = _source[_pos];

        // Comment: * at start of line (column 1) or ; anywhere
        if (c == ';' || (c == '*' && _col == 1))
            return LexLineComment();

        if (c == '\r' || c == '\n')
            return LexNewline();

        if (c == '"' || c == '\'')
            return LexString(c);

        if (char.IsDigit(c))
            return LexNumber();

        if (char.IsLetter(c) || c == '_')
            return LexIdentifierOrKeyword();

        return LexSymbol();
    }

    private Token LexLineComment()
    {
        int startLine = _line;
        int startCol  = _col;
        var sb = new System.Text.StringBuilder();
        while (_pos < _source.Length && _source[_pos] != '\r' && _source[_pos] != '\n')
        {
            sb.Append(_source[_pos]);
            Advance();
        }
        return new Token(TokenType.Comment, sb.ToString(), startLine, startCol);
    }

    private Token LexNewline()
    {
        int startLine = _line;
        int startCol  = _col;
        if (_source[_pos] == '\r')
        {
            Advance();
            if (_pos < _source.Length && _source[_pos] == '\n')
                Advance();
        }
        else
        {
            Advance();
        }
        _line++;
        _col = 1;
        return new Token(TokenType.Newline, "\\n", startLine, startCol);
    }

    private Token LexString(char quote)
    {
        int startLine = _line;
        int startCol  = _col;
        Advance(); // skip opening quote
        var sb = new System.Text.StringBuilder();
        while (_pos < _source.Length && _source[_pos] != quote)
        {
            sb.Append(_source[_pos]);
            Advance();
        }
        if (_pos < _source.Length) Advance(); // closing quote
        return new Token(TokenType.String, sb.ToString(), startLine, startCol);
    }

    private Token LexNumber()
    {
        int startLine = _line;
        int startCol  = _col;
        var sb = new System.Text.StringBuilder();
        bool isFloat = false;
        while (_pos < _source.Length && (char.IsDigit(_source[_pos]) || _source[_pos] == '.'))
        {
            if (_source[_pos] == '.') isFloat = true;
            sb.Append(_source[_pos]);
            Advance();
        }
        return new Token(isFloat ? TokenType.Float : TokenType.Integer, sb.ToString(), startLine, startCol);
    }

    private Token LexIdentifierOrKeyword()
    {
        int startLine = _line;
        int startCol  = _col;
        var sb = new System.Text.StringBuilder();
        while (_pos < _source.Length && (char.IsLetterOrDigit(_source[_pos]) || _source[_pos] == '_' || _source[_pos] == '$'))
        {
            sb.Append(_source[_pos]);
            Advance();
        }
        string text = sb.ToString();
        var type = Keywords.TryGetValue(text, out var kw) ? kw : TokenType.Identifier;
        return new Token(type, text, startLine, startCol);
    }

    private Token LexSymbol()
    {
        int startLine = _line;
        int startCol  = _col;
        char c = _source[_pos];
        Advance();
        var type = c switch
        {
            ',' => TokenType.Comma,
            '+' => TokenType.Plus,
            '-' => TokenType.Minus,
            '*' => TokenType.Star,
            '/' => TokenType.Slash,
            '#' => TokenType.Hash,
            '@' => TokenType.At,
            '$' => TokenType.Dollar,
            '&' => TokenType.Ampersand,
            '%' => TokenType.Percent,
            '^' => TokenType.Caret,
            '(' => TokenType.OpenParen,
            ')' => TokenType.CloseParen,
            ':' => TokenType.Colon,
            ';' => TokenType.SemiColon,
            _   => TokenType.Unknown,
        };
        return new Token(type, c.ToString(), startLine, startCol);
    }

    private Token Make(TokenType type, string value) =>
        new Token(type, value, _line, _col);

    private void Advance()
    {
        _pos++;
        _col++;
    }
}
