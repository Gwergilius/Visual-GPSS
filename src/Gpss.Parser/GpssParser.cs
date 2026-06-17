using Gpss.Contracts;
using Gpss.Model;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;

namespace Gpss.Parser;

/// <summary>
/// Parses GPSS source text into a <see cref="GpssProgram"/> AST.
/// </summary>
/// <remarks>
/// Supported syntax:
/// <code>
/// [label]  BLOCK_NAME  [A[,B[,C[,D[,E]]]]]  [; comment]
/// </code>
/// Lines that are empty, whitespace-only, or begin with <c>;</c> (inline) or <c>*</c> (full-line) are ignored.
/// The <c>END</c> statement terminates parsing; further lines are not processed.
/// Recognised block names are listed in <see cref="KnownGpssBlocks"/>.
/// </remarks>
public sealed class GpssParser
{
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
        var lineNumber = 0;

        using var reader = new StringReader(sourceText);
        string? rawLine;

        while ((rawLine = reader.ReadLine()) != null)
        {
            lineNumber++;

            // Strip inline comment (semicolon and everything after)
            var commentIdx = rawLine.IndexOf(';');
            var line = (commentIdx >= 0 ? rawLine[..commentIdx] : rawLine).Trim();

            if (string.IsNullOrEmpty(line)) continue;

            // Full-line comment: * as the first non-whitespace character (GPSS column-1 convention)
            if (line[0] == '*') continue;

            // Split into whitespace-delimited tokens
            var tokens = line.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0) continue;

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

            var block = BuildBlock(blockName, label, operands, lineNumber, diagnostics);
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

    private static GpssBlock? BuildBlock(
        string blockName, string? label, IReadOnlyList<string?> operands,
        int lineNumber, List<DiagnosticMessage> diagnostics) =>
        blockName switch
        {
            "GENERATE" => BuildGenerateBlock(label, operands, lineNumber, diagnostics),
            "ADVANCE" => BuildAdvanceBlock(label, operands, lineNumber, diagnostics),
            "TERMINATE" => BuildTerminateBlock(label, operands, lineNumber, diagnostics),
            "SEIZE"     => BuildFacilityBlock<SeizeBlock>(label, operands, lineNumber, "SEIZE", diagnostics,
                               name => new SeizeBlock(new SymbolExpression(name))),
            "RELEASE"   => BuildFacilityBlock<ReleaseBlock>(label, operands, lineNumber, "RELEASE", diagnostics,
                               name => new ReleaseBlock(new SymbolExpression(name))),
            "QUEUE"     => BuildFacilityBlock<QueueBlock>(label, operands, lineNumber, "QUEUE", diagnostics,
                               name => new QueueBlock(new SymbolExpression(name))),
            "DEPART"    => BuildFacilityBlock<DepartBlock>(label, operands, lineNumber, "DEPART", diagnostics,
                               name => new DepartBlock(new SymbolExpression(name))),
            _ => null
        };

    private static GenerateBlock? BuildGenerateBlock(
        string? label, IReadOnlyList<string?> operands,
        int lineNumber, List<DiagnosticMessage> diagnostics)
    {
        if (operands.Count == 0 || operands[0] is null)
        {
            diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
                $"Line {lineNumber}: GENERATE requires operand A (mean inter-arrival time)."));
            return null;
        }

        var mean = ParseIntExpr(operands[0]!, lineNumber, "A", diagnostics);
        if (mean is null) return null;

        return new GenerateBlock(
            mean,
            Operand(operands, 1, lineNumber, "B", diagnostics),
            Operand(operands, 2, lineNumber, "C", diagnostics),
            Operand(operands, 3, lineNumber, "D", diagnostics),
            Operand(operands, 4, lineNumber, "E", diagnostics))
        { Label = label };
    }

    private static AdvanceBlock BuildAdvanceBlock(
        string? label, IReadOnlyList<string?> operands,
        int lineNumber, List<DiagnosticMessage> diagnostics) =>
        new(
            Operand(operands, 0, lineNumber, "A", diagnostics),
            Operand(operands, 1, lineNumber, "B", diagnostics))
        { Label = label };

    private static TerminateBlock BuildTerminateBlock(
        string? label, IReadOnlyList<string?> operands,
        int lineNumber, List<DiagnosticMessage> diagnostics) =>
        new(Operand(operands, 0, lineNumber, "A", diagnostics)) { Label = label };

    private static GpssExpression? Operand(
        IReadOnlyList<string?> operands, int index,
        int lineNumber, string name, List<DiagnosticMessage> diagnostics) =>
        index < operands.Count && operands[index] is { } text
            ? ParseIntExpr(text, lineNumber, name, diagnostics)
            : null;

    private static TBlock? BuildFacilityBlock<TBlock>(
        string? label, IReadOnlyList<string?> operands,
        int lineNumber, string blockName,
        List<DiagnosticMessage> diagnostics,
        Func<string, TBlock> factory)
        where TBlock : GpssBlock
    {
        if (operands.Count == 0 || operands[0] is null)
        {
            diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
                $"Line {lineNumber}: {blockName} requires operand A (facility name)."));
            return null;
        }

        return factory(operands[0]!) is { } block
            ? (TBlock)((GpssBlock)block with { Label = label })
            : null;
    }

    private static IntegerExpression? ParseIntExpr(
        string text, int lineNumber, string operandName,
        List<DiagnosticMessage> diagnostics)
    {
        if (int.TryParse(text, out var value)) return new IntegerExpression(value);

        diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Error,
            $"Line {lineNumber}: operand {operandName} '{text}' is not a valid integer."));
        return null;
    }
}
