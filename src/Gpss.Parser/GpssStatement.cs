using Gpss.Contracts;

namespace Gpss.Parser;

/// <summary>
/// A single GPSS statement decoded from the source by <see cref="GpssReader.Read"/>.
/// </summary>
/// <param name="FileName">
/// Path or name of the source file this statement was read from. Reflects whichever file was
/// active when the statement was read, taking <c>INCLUDE</c> into account.
/// </param>
/// <param name="FileNumber">
/// 1-based ordinal identifying the file this statement was read from. Assigned in the order
/// files are opened: the initial file is always <c>1</c>, and each <c>INCLUDE</c> target
/// receives the next number, even when the same file is included more than once.
/// </param>
/// <param name="LineNumber">1-based source line number within <see cref="FileName"/>.</param>
/// <param name="Label">The statement's label, or <see langword="null"/> when absent.</param>
/// <param name="Verb">The block keyword (or control statement) for this statement, upper-cased.</param>
/// <param name="Operands">
/// Parsed operand tokens (A, B, C, ...); <see langword="null"/> marks a skipped slot (e.g. the
/// middle slot in <c>10,,5</c>). An operand quoted with <c>"..."</c> or <c>'...'</c> is unquoted
/// and kept verbatim, including any comma or whitespace it contains.
/// </param>
/// <param name="Comment">Trailing inline comment text (after <c>;</c>), or <see langword="null"/> when absent.</param>
public sealed record GpssStatement(
    string FileName,
    int FileNumber,
    int LineNumber,
    string? Label,
    string Verb,
    IReadOnlyList<string?> Operands,
    string? Comment) : ISourceLocation;
