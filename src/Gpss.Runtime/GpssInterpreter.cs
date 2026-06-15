using Gpss.Runtime.Simulation;
using LexerNs = Gpss.Runtime.Lexer;
using ParserNs = Gpss.Runtime.Parser;

namespace Gpss.Runtime;

/// <summary>
/// High-level entry point for running a GPSS program.
/// </summary>
/// <example>
/// <code>
/// var report = GpssInterpreter.Run("""
///     GENERATE  10,2
///     QUEUE     LOBBY
///     SEIZE     CASHIER
///     DEPART    LOBBY
///     ADVANCE   5,1
///     RELEASE   CASHIER
///     TERMINATE 1
///
///     START 1000
///     END
///     """);
/// Console.WriteLine(report);
/// </code>
/// </example>
public static class GpssInterpreter
{
    /// <summary>
    /// Tokenizes, parses, and runs a GPSS source program.
    /// Returns the simulation report.
    /// </summary>
    /// <param name="source">The GPSS source text.</param>
    /// <param name="seed">Random seed (0 = non-deterministic).</param>
    public static SimulationReport Run(string source, int seed = 0)
    {
        var (model, _) = Build(source, seed);
        return model.Run();
    }

    /// <summary>
    /// Builds (but does not run) a <see cref="SimulationModel"/> from GPSS source.
    /// Useful for introspection and visual editing.
    /// </summary>
    /// <param name="source">The GPSS source text.</param>
    /// <param name="seed">Random seed (0 = non-deterministic).</param>
    /// <returns>The loaded model and the parsed statements.</returns>
    public static (SimulationModel Model, IReadOnlyList<ParserNs.Statement> Statements) Build(
        string source, int seed = 0)
    {
        ArgumentNullException.ThrowIfNull(source);

        var lexer      = new LexerNs.Lexer(source);
        var tokens     = lexer.Tokenize();

        var parser     = new ParserNs.Parser(tokens);
        var statements = parser.Parse();

        var model      = new SimulationModel(seed);
        model.Load(statements);

        return (model, statements);
    }

    /// <summary>
    /// Tokenizes and parses the source without executing, returning
    /// the AST statements for inspection.
    /// </summary>
    public static IReadOnlyList<ParserNs.Statement> Parse(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var lexer  = new LexerNs.Lexer(source);
        var tokens = lexer.Tokenize();
        return new ParserNs.Parser(tokens).Parse();
    }
}
