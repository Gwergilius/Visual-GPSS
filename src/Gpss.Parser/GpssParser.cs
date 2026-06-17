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
/// Lines that are empty, whitespace-only, or begin with <c>;</c> (inline) or <c>*</c> (full-line) are ignored
/// (filtered out by <see cref="GpssReader"/>).
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
    /// <returns>
    /// A result containing the parsed program on success,
    /// or <see langword="null"/> for <see cref="GpssParseResult.Program"/> when errors were found.
    /// </returns>
    public GpssParseResult Parse(string sourceText)
    {
        var blocks = new List<GpssBlock>();
        var diagnostics = new List<DiagnosticMessage>();

        using var reader = new GpssReader(new StringReader(sourceText));
        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            var lineNumber = reader.LineNumber;

            // Split into whitespace-delimited tokens
            var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

            // Detect END statement
            if (tokens[0].Equals("END", StringComparison.OrdinalIgnoreCase)) break;

            // Distinguish label from block name:
            // if the second token is a known block name, the first is the label
            string? label;
            string blockName;
            int operandTokenStart;

            if (tokens.Length >= 2 && IsBlockName(tokens[1]))
            {
                label = tokens[0];
                blockName = tokens[1].ToUpperInvariant();
                operandTokenStart = 2;
            }
            else if (IsBlockName(tokens[0]))
            {
                label = null;
                blockName = tokens[0].ToUpperInvariant();
                operandTokenStart = 1;
            }
            else
            {
                diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
                    $"Line {lineNumber}: '{tokens[0]}' is not a recognised block name."));
                continue;
            }

            // Collect operand tokens and join without spaces, then split by comma.
            // This handles both "GENERATE 10,3" and "GENERATE 10 , 3" correctly.
            var operandString = operandTokenStart < tokens.Length
                ? string.Join("", tokens[operandTokenStart..])
                : string.Empty;

            var operands = SplitOperands(operandString);

            var block = _builders.For(blockName).Build(label, operands, lineNumber, diagnostics);
            if (block != null) blocks.Add(block);
        }

        return diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)
            ? new GpssParseResult(null, diagnostics)
            : new GpssParseResult(new GpssProgram(blocks), diagnostics);
    }

    private static bool IsBlockName(string token) => KnownGpssBlocks.IsKnown(token);

    /// <summary>
    /// Splits a comma-delimited operand string into individual operand strings.
    /// Returns <see langword="null"/> for empty slots (e.g. the middle slot in <c>10,,5</c>).
    /// </summary>
    private static IReadOnlyList<string?> SplitOperands(string operandString)
    {
        if (string.IsNullOrEmpty(operandString)) return [];

        return operandString.Split(',')
            .Select(p => { var t = p.Trim(); return string.IsNullOrEmpty(t) ? null : t; })
            .ToArray();
    }
}
