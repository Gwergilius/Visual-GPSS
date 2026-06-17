namespace Gpss.Parser;

/// <summary>
/// Reads GPSS source text line by line, filtering out comments and blank lines so that
/// callers only see "useful" content lines.
/// </summary>
/// <remarks>
/// Wraps a <see cref="TextReader"/> (typically a <see cref="StringReader"/>). Lines that are
/// empty or whitespace-only after trimming, or begin with <c>*</c> (full-line comment, GPSS
/// column-1 convention), are skipped entirely. Inline comments (<c>;</c> and everything after
/// it) are stripped from each line before it is returned.
/// </remarks>
public sealed class GpssReader : IDisposable
{
    private readonly TextReader _inner;

    /// <summary>
    /// Initializes a new instance wrapping the given <paramref name="inner"/> text reader.
    /// </summary>
    /// <param name="inner">The underlying text reader to read raw lines from.</param>
    public GpssReader(TextReader inner)
    {
        _inner = inner;
    }

    /// <summary>
    /// Gets the 1-based source line number of the line most recently returned by <see cref="ReadLine"/>.
    /// </summary>
    public int LineNumber { get; private set; }

    /// <summary>
    /// Reads the next non-empty, non-comment line from the source, with any inline comment stripped.
    /// </summary>
    /// <returns>The trimmed line content, or <see langword="null"/> at end of input.</returns>
    public string? ReadLine()
    {
        string? rawLine;
        while ((rawLine = _inner.ReadLine()) != null)
        {
            LineNumber++;

            // Strip inline comment (semicolon and everything after)
            var commentIdx = rawLine.IndexOf(';');
            var line = (commentIdx >= 0 ? rawLine[..commentIdx] : rawLine).Trim();

            if (string.IsNullOrEmpty(line)) continue;

            // Full-line comment: * as the first non-whitespace character (GPSS column-1 convention)
            if (line[0] == '*') continue;

            return line;
        }

        return null;
    }

    /// <summary>
    /// Disposes the underlying <see cref="TextReader"/>.
    /// </summary>
    public void Dispose() => _inner.Dispose();
}
