namespace Gpss.Contracts;

/// <summary>A source location (file and line) that a <see cref="DiagnosticMessage"/> can be attributed to.</summary>
public interface ISourceLocation
{
    /// <summary>Name or path of the source file.</summary>
    string FileName { get; }

    /// <summary>1-based line number within <see cref="FileName"/>.</summary>
    int LineNumber { get; }

    /// <summary>
    /// The trailing comment on the line at this location, or <see langword="null"/> when absent.
    /// Carried onto any <see cref="DiagnosticMessage"/> raised about this location, so the
    /// author's explanation of intent stays attached even to an error or warning about that line.
    /// </summary>
    string? Comment { get; }
}
