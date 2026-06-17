using Gpss.Contracts;
using Gpss.Model;
using Gpss.Model.Blocks;
using Gpss.Parser.Internal;

namespace Gpss.Parser;

/// <summary>
/// Parses GPSS source text into a <see cref="GpssProgram"/> AST.
/// </summary>
/// <remarks>
/// Supported syntax:
/// <code>
/// [label]  BLOCK_NAME  [A[,B[,C[,D[,E]]]]]  [; comment]
/// </code>
/// The block name is never in column 1: a label, when present, occupies column 1, so a
/// label-less statement must be indented (see <see cref="GpssReader"/> for the full lexical rule).
/// Lines that are empty, whitespace-only, or begin with <c>;</c> (inline) or <c>*</c> (full-line) are ignored,
/// and <c>INCLUDE</c> statements are followed transparently, all handled by <see cref="GpssReader"/>.
/// The <c>END</c> statement terminates parsing; further lines are not processed.
/// Recognised block names are listed in <see cref="KnownGpssBlocks"/>.
/// Acts as a Mediator: dispatches each recognised block name to its registered
/// <see cref="IBlockBuilder"/> via <see cref="BlockBuilderRegistry"/>, without the caller needing
/// to know which builder produces which block type.
/// The constructor is <see langword="internal"/> because it takes an internal type.
/// Obtain instances via the DI container (see <see cref="ParserServiceCollectionExtensions.AddGpssParser"/>).
/// </remarks>
public sealed class GpssParser
{
    private readonly BlockBuilderRegistry _builders;

    /// <summary>
    /// Initialises the parser. Use <see cref="ParserServiceCollectionExtensions.AddGpssParser"/>
    /// to obtain instances via DI.
    /// </summary>
    /// <param name="builders">Registry mapping block keywords to their builders.</param>
    internal GpssParser(BlockBuilderRegistry builders)
    {
        _builders = builders;
    }

    /// <summary>
    /// Parses <paramref name="sourceText"/> and returns a <see cref="GpssParseResult"/>.
    /// </summary>
    /// <param name="sourceText">GPSS source text to parse.</param>
    /// <param name="fileName">
    /// Name or path of <paramref name="sourceText"/>'s source file. Used to resolve relative
    /// <c>INCLUDE</c> targets and to populate file information in diagnostics; defaults to the
    /// empty string when the source has no associated file.
    /// </param>
    /// <returns>
    /// A result containing the parsed program on success,
    /// or <see langword="null"/> for <see cref="GpssParseResult.Program"/> when errors were found.
    /// </returns>
    public GpssParseResult Parse(string sourceText, string fileName = "")
    {
        var blocks = new List<GpssBlock>();
        var diagnostics = new List<DiagnosticMessage>();

        using var reader = new GpssReader(new StringReader(sourceText), fileName);
        GpssStatement? statement;

        while ((statement = reader.Read()) != null)
        {
            // Detect END statement
            if (statement.Verb.Equals("END", StringComparison.OrdinalIgnoreCase)) break;

            if (!IsBlockName(statement.Verb))
            {
                diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
                    $"'{statement.Verb}' is not a recognised block name.", statement));
                continue;
            }

            if (statement.Comment is { } comment)
            {
                // The comment is the Message here, so it isn't also passed via the
                // ISourceLocation overload, which would duplicate it onto DiagnosticMessage.Comment.
                diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Info, comment,
                    statement.FileName, statement.LineNumber));
            }

            var block = _builders.For(statement.Verb).Build(statement, diagnostics);
            if (block != null) blocks.Add(block);
        }

        return diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)
            ? new GpssParseResult(null, diagnostics)
            : new GpssParseResult(new GpssProgram(blocks), diagnostics);
    }

    private static bool IsBlockName(string token) => KnownGpssBlocks.IsKnown(token);
}
