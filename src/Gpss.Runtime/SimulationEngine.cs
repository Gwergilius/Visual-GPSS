using Gpss.Contracts;
using Gpss.Model;
using Gpss.Runtime.Internal;
using Gpss.Runtime.Internal.Behaviours;
using Microsoft.Extensions.Logging;

namespace Gpss.Runtime;

/// <summary>
/// Orchestrates a discrete-event GPSS simulation as a Mediator:
/// coordinates <see cref="IBlockBehaviour"/> implementations, the Future Events Chain,
/// and the simulation clock without coupling any of them directly to each other.
/// The engine is stateless; each call to <see cref="Run"/> is fully independent.
/// </summary>
/// <remarks>
/// The constructor is <see langword="internal"/> because it takes internal types.
/// Obtain instances via the DI container (see <see cref="RuntimeServiceCollectionExtensions.AddGpssRuntime"/>).
/// </remarks>
public sealed class SimulationEngine
{
    private readonly BlockBehaviourRegistry _registry;
    private readonly ILogger<SimulationEngine> _logger;

    /// <summary>Initialises the engine. Use <see cref="RuntimeServiceCollectionExtensions.AddGpssRuntime"/> to obtain instances via DI.</summary>
    /// <param name="registry">Registry mapping block types to their behaviours.</param>
    /// <param name="logger">Logger for simulation lifecycle events.</param>
    internal SimulationEngine(BlockBehaviourRegistry registry, ILogger<SimulationEngine> logger)
    {
        _registry = registry;
        _logger = logger;
    }

    /// <summary>
    /// Executes <paramref name="program"/> as a discrete-event simulation and returns the result.
    /// </summary>
    /// <remarks>
    /// The simulation runs until one of the following conditions is met:
    /// <list type="bullet">
    ///   <item>The termination counter reaches zero (normal end).</item>
    ///   <item>The Future Events Chain is exhausted with no termination.</item>
    ///   <item><see cref="SimulationOptions.MaxEvents"/> is exceeded (safety limit).</item>
    /// </list>
    /// </remarks>
    /// <param name="program">The parsed GPSS model to execute.</param>
    /// <param name="options">Run-time parameters including the initial termination count.</param>
    /// <returns>A <see cref="SimulationResult"/> describing the outcome and statistics.</returns>
    public SimulationResult Run(GpssProgram program, SimulationOptions options)
    {
        if (options.TerminationCount <= 0)
            return BuildResult(success: true, clock: 0.0, created: 0, terminated: 0, []);

        _logger.LogInformation(
            "Simulation starting — termination count: {Count}, max events: {MaxEvents}",
            options.TerminationCount, options.MaxEvents?.ToString() ?? "unlimited");

        var context = new SimulationContext(options.TerminationCount);
        var blockContexts = BuildBlockContexts(program);
        var eventsProcessed = 0L;
        var diagnostics = new List<DiagnosticMessage>();

        // Initialise all blocks (e.g. GENERATE schedules its first transaction)
        foreach (var bc in blockContexts)
            _registry.For(bc.Block).OnSimulationStart(bc, context);

        // Main simulation loop — each iteration processes one FEC event
        while (!context.IsTerminated && context.TryDequeueNext(out var tx))
        {
            if (options.MaxEvents.HasValue && eventsProcessed >= options.MaxEvents.Value)
            {
                var msg = $"Simulation stopped after reaching the maximum event limit ({options.MaxEvents.Value}).";
                _logger.LogWarning(msg);
                diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Warning, msg));
                return BuildResult(success: false, context.Clock,
                    context.TotalTransactionsCreated, context.TotalTransactionsTerminated, diagnostics);
            }

            context.AdvanceClock(tx.ScheduledTime);

            // CEC pass: move the transaction through consecutive blocks until it is
            // destroyed, delayed into the FEC, or reaches the end of the program
            var result = BlockTransactionResult.Moved;
            while (result == BlockTransactionResult.Moved && tx.BlockIndex < program.Blocks.Count)
            {
                var bc = blockContexts[tx.BlockIndex];
                result = _registry.For(bc.Block).OnTransactionArrival(bc, tx, context);
            }

            eventsProcessed++;
        }

        _logger.LogInformation(
            "Simulation ended at t={Clock} — created: {Created}, terminated: {Terminated}",
            context.Clock, context.TotalTransactionsCreated, context.TotalTransactionsTerminated);

        return BuildResult(success: true, context.Clock,
            context.TotalTransactionsCreated, context.TotalTransactionsTerminated, diagnostics);
    }

    /// <summary>Builds a <see cref="BlockContext"/> list — one entry per block, in program order.</summary>
    private static List<BlockContext> BuildBlockContexts(GpssProgram program) =>
        program.Blocks.Select((b, i) => new BlockContext(b, i)).ToList();

    private static SimulationResult BuildResult(
        bool success, double clock, long created, long terminated,
        List<DiagnosticMessage> diagnostics) =>
        new(success, new SimulationStatistics(clock, created, terminated), diagnostics);
}
