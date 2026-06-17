using System.Text;

namespace Gpss.Parser;

/// <summary>
/// Reads GPSS source text and decodes it into a stream of <see cref="GpssStatement"/> records,
/// filtering out comments and blank lines and transparently following <c>INCLUDE</c> statements.
/// </summary>
/// <remarks>
/// Wraps a <see cref="TextReader"/> (typically a <see cref="StringReader"/> for the initial,
/// top-level source). Lines that are empty or whitespace-only after stripping any inline comment,
/// or that begin with <c>*</c> (full-line comment, GPSS column-1 convention), are skipped entirely.
/// <para>
/// A line is lexed as <c>[label] verb [operand (',' operand)*] [';' comment]</c>. <c>label</c> and
/// <c>verb</c> are identifier-like words (a letter or <c>_</c> followed by letters, digits, or
/// <c>_</c>). The verb is always preceded by at least one whitespace character and so never starts
/// in the line's first column: a label, when present, occupies that first column, and a line with
/// no label must therefore be indented. This is a purely positional rule — no GPSS keyword
/// knowledge is needed to tell a label from a verb — so a flush-left, unindented, label-less line
/// (e.g. <c>TERMINATE 1</c> starting at column 1 with nothing before it) is malformed: it is read
/// as label <c>TERMINATE</c> with verb <c>1</c>, which is then rejected downstream as an unknown
/// block name. Each <c>operand</c> is either a bare word/expression (any run of text not containing
/// a comma, trimmed of surrounding whitespace) or a <c>"..."</c>- or <c>'...'</c>-quoted string,
/// whose contents — including commas and whitespace — are taken verbatim once the quotes are
/// stripped. Quoting therefore lets a single operand contain a comma or embedded whitespace, which
/// is what allows <c>INCLUDE</c>'s file name operand to be a path containing spaces.
/// </para>
/// <para>
/// <c>INCLUDE filename</c> statements (see the GPSS reference manual) are handled internally and
/// never surfaced to the caller: their sole operand is read like any other operand (so it may be
/// quoted), the current file and position are pushed onto a stack, the named file is opened and
/// read from its first line, and once it reaches end-of-file the previous file and position are
/// popped back off the stack so reading resumes at the line after the <c>INCLUDE</c>. By default
/// included files are opened from disk via <see cref="File.OpenText"/>, resolving relative names
/// against the directory of the file containing the <c>INCLUDE</c> statement; pass
/// <paramref name="includeFileOpener"/> to override this (e.g. in tests).
/// </para>
/// </remarks>
public sealed class GpssReader : IDisposable
{
    private readonly Func<string, TextReader> _includeFileOpener;
    private readonly Stack<Frame> _stack = new();
    private Frame _current;
    private int _nextFileNumber;

    /// <summary>
    /// Initializes a new instance wrapping the given <paramref name="inner"/> text reader as the
    /// initial (top-level) source file.
    /// </summary>
    /// <param name="inner">The underlying text reader to read raw lines from.</param>
    /// <param name="fileName">
    /// Name or path of the source represented by <paramref name="inner"/>, used to populate
    /// <see cref="GpssStatement.FileName"/> and to resolve relative <c>INCLUDE</c> targets.
    /// Defaults to the empty string when the source has no associated file.
    /// </param>
    /// <param name="includeFileOpener">
    /// Opens an included file given its resolved name; defaults to <see cref="File.OpenText"/>.
    /// Overridable so tests can supply included content without touching the file system.
    /// </param>
    public GpssReader(TextReader inner, string fileName = "", Func<string, TextReader>? includeFileOpener = null)
    {
        _includeFileOpener = includeFileOpener ?? (path => File.OpenText(path));
        _current = new Frame(inner, fileName, NextFileNumber());
    }

    /// <summary>Gets the name of the file currently being read (see <see cref="GpssStatement.FileName"/>).</summary>
    public string FileName => _current.FileName;

    /// <summary>Gets the ordinal of the file currently being read (see <see cref="GpssStatement.FileNumber"/>).</summary>
    public int FileNumber => _current.FileNumber;

    /// <summary>Gets the 1-based line number, within the current file, of the line most recently read.</summary>
    public int LineNumber => _current.LineNumber;

    /// <summary>
    /// Reads the next GPSS statement from the source, skipping blank lines and comments and
    /// transparently following any <c>INCLUDE</c> statements encountered along the way.
    /// </summary>
    /// <returns>The next decoded statement, or <see langword="null"/> at the end of the top-level input.</returns>
    public GpssStatement? Read()
    {
        while (true)
        {
            var rawLine = _current.Reader.ReadLine();
            if (rawLine is null)
            {
                if (_stack.Count == 0) return null;

                _current.Reader.Dispose();
                _current = _stack.Pop();
                continue;
            }

            _current.LineNumber++;

            // The comment delimiter only counts outside of a quoted operand, so that a ';' inside
            // a quoted string (e.g. a path) is not mistaken for the start of a comment.
            var commentIdx = FindUnquoted(rawLine, ';');
            var rawContent = commentIdx >= 0 ? rawLine[..commentIdx] : rawLine;
            var comment = commentIdx >= 0 ? rawLine[(commentIdx + 1)..].Trim() : null;
            if (comment is { Length: 0 }) comment = null;

            // The verb is always preceded by at least one whitespace character; a label, when
            // present, occupies column 1. So "no leading whitespace" means "this line has a label".
            var hasLabel = rawContent.Length > 0 && !char.IsWhiteSpace(rawContent[0]);
            var content = rawContent.Trim();

            if (string.IsNullOrEmpty(content)) continue;
            if (content[0] == '*') continue;

            var (label, verb, operandRegionStart) = ReadLabelAndVerb(content, hasLabel);
            var operands = SplitOperands(content[operandRegionStart..]);

            if (verb.Equals("INCLUDE", StringComparison.OrdinalIgnoreCase))
            {
                if (operands.Count == 0 || operands[0] is null)
                    throw new FormatException(
                        $"'{_current.FileName}' line {_current.LineNumber}: INCLUDE requires a file name operand.");

                HandleInclude(operands[0]!);
                continue;
            }

            return new GpssStatement(
                _current.FileName, _current.FileNumber, _current.LineNumber,
                label, verb, operands, comment);
        }
    }

    /// <summary>
    /// Disposes the text readers for the current file and any files still pending on the
    /// <c>INCLUDE</c> stack.
    /// </summary>
    public void Dispose()
    {
        _current.Reader.Dispose();
        while (_stack.Count > 0) _stack.Pop().Reader.Dispose();
    }

    /// <summary>
    /// Reads the optional label and the verb word from the start of <paramref name="content"/>.
    /// Purely positional: <paramref name="hasLabel"/> tells whether the line had no leading
    /// whitespace (so its first word is a label, and the verb is the word after it) or was
    /// indented (so there is no label, and the first word is the verb itself).
    /// </summary>
    /// <returns>
    /// The label (or <see langword="null"/>), the upper-cased verb, and the index in
    /// <paramref name="content"/> where the operand list begins.
    /// </returns>
    private static (string? Label, string Verb, int OperandRegionStart) ReadLabelAndVerb(string content, bool hasLabel)
    {
        var firstEnd = SkipWord(content, 0);

        if (!hasLabel)
            return (null, content[..firstEnd].ToUpperInvariant(), SkipWhitespace(content, firstEnd));

        var afterLabel = SkipWhitespace(content, firstEnd);
        var verbEnd = SkipWord(content, afterLabel);
        return (content[..firstEnd], content[afterLabel..verbEnd].ToUpperInvariant(),
            SkipWhitespace(content, verbEnd));
    }

    /// <summary>
    /// Splits an operand list into individual operand strings.
    /// </summary>
    /// <remarks>
    /// Grammar: <c>operand: (char | string)*</c>, where <c>char</c> is anything other than
    /// <c>,</c>, <c>;</c>, <c>'</c>, <c>"</c>, or end-of-line, and <c>string</c> is a
    /// <c>"..."</c>- or <c>'...'</c>-quoted run whose content — anything but the matching quote
    /// character — is taken verbatim. An operand may freely mix bare and quoted runs (e.g.
    /// <c>ab"c,d"ef</c> is the single operand <c>abc,def</c>): quote characters always introduce
    /// or close a quoted run and never appear in the result themselves, regardless of where in the
    /// operand they occur. This is what an operand that is itself a string expression needs, e.g.
    /// <c>ToUpper("lower case sentence")</c> — the quoting only protects the string literal
    /// argument (which may contain commas or whitespace); the surrounding call syntax is bare
    /// characters in the same operand. Operands are separated by <c>,</c> (with any amount of
    /// whitespace allowed around it); reading an operand list stops at an unquoted <c>;</c> or at
    /// the end of the input. Each operand is trimmed before being returned; <see langword="null"/>
    /// marks an empty slot (e.g. the middle slot in <c>10,,5</c>).
    /// </remarks>
    private IReadOnlyList<string?> SplitOperands(string operandRegion)
    {
        if (string.IsNullOrWhiteSpace(operandRegion)) return [];

        var operands = new List<string?>();
        var current = new StringBuilder();
        var i = 0;

        while (i < operandRegion.Length)
        {
            var c = operandRegion[i];

            if (c is '"' or '\'')
            {
                var closeIdx = operandRegion.IndexOf(c, i + 1);
                if (closeIdx < 0)
                    throw new FormatException(
                        $"'{_current.FileName}' line {_current.LineNumber}: unterminated {c} in operand list.");

                current.Append(operandRegion[(i + 1)..closeIdx]);
                i = closeIdx + 1;
                continue;
            }

            if (c == ',')
            {
                operands.Add(FinishOperand(current));
                i++;
                continue;
            }

            if (c == ';') break; // comment marker; already stripped upstream in normal use

            current.Append(c);
            i++;
        }

        operands.Add(FinishOperand(current));
        return operands;
    }

    /// <summary>Trims the accumulated operand text and resets <paramref name="buffer"/> for reuse.</summary>
    /// <returns>The trimmed operand, or <see langword="null"/> when it is empty.</returns>
    private static string? FinishOperand(StringBuilder buffer)
    {
        var text = buffer.ToString().Trim();
        buffer.Clear();
        return text.Length == 0 ? null : text;
    }

    /// <summary>Returns the index of the first <paramref name="target"/> in <paramref name="s"/> that is not inside a quoted span, or -1.</summary>
    private static int FindUnquoted(string s, char target)
    {
        char? quote = null;
        for (var i = 0; i < s.Length; i++)
        {
            var c = s[i];
            if (quote is { } q)
            {
                if (c == q) quote = null;
                continue;
            }

            if (c is '"' or '\'') { quote = c; continue; }
            if (c == target) return i;
        }

        return -1;
    }

    /// <summary>
    /// Returns the end index of the identifier-like word starting at <paramref name="start"/>
    /// (a letter or <c>_</c> followed by letters, digits, or <c>_</c>). When <paramref name="start"/>
    /// is not a valid word start, falls back to the next whitespace run so malformed input still
    /// yields a usable token instead of an empty one.
    /// </summary>
    private static int SkipWord(string s, int start)
    {
        var i = start;
        if (i < s.Length && (char.IsLetter(s[i]) || s[i] == '_'))
        {
            i++;
            while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_')) i++;
            return i;
        }

        while (i < s.Length && !char.IsWhiteSpace(s[i])) i++;
        return i;
    }

    private static int SkipWhitespace(string s, int start)
    {
        var i = start;
        while (i < s.Length && char.IsWhiteSpace(s[i])) i++;
        return i;
    }

    private void HandleInclude(string includeFileName)
    {
        var resolvedName = ResolveIncludePath(_current.FileName, includeFileName);
        var reader = _includeFileOpener(resolvedName);

        _stack.Push(_current);
        _current = new Frame(reader, resolvedName, NextFileNumber());
    }

    private static string ResolveIncludePath(string currentFileName, string includeFileName)
    {
        if (Path.IsPathRooted(includeFileName) || string.IsNullOrEmpty(currentFileName)) return includeFileName;

        var dir = Path.GetDirectoryName(currentFileName);
        return string.IsNullOrEmpty(dir) ? includeFileName : Path.Combine(dir, includeFileName);
    }

    private int NextFileNumber() => ++_nextFileNumber;

    /// <summary>Tracks the text reader and read position for one open file in the <c>INCLUDE</c> stack.</summary>
    private sealed class Frame(TextReader reader, string fileName, int fileNumber)
    {
        /// <summary>The text reader for this file.</summary>
        public TextReader Reader { get; } = reader;

        /// <summary>The file's name, as recorded on <see cref="GpssStatement.FileName"/>.</summary>
        public string FileName { get; } = fileName;

        /// <summary>The file's ordinal, as recorded on <see cref="GpssStatement.FileNumber"/>.</summary>
        public int FileNumber { get; } = fileNumber;

        /// <summary>The 1-based line number of the line most recently read from <see cref="Reader"/>.</summary>
        public int LineNumber { get; set; }
    }
}
