using Gpss.Contracts;
using Gpss.Model;
using Gpss.Runtime.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gpss.Runtime;

/// <summary>
/// Orchestrates a discrete-event GPSS simulation as a Mediator:
/// coordinates <see cref="IBlockBehaviour"/> implementations, the Future Events Chain,
/// and the simulation clock without coupling any of them directly to each other.
/// Simulation parameters are supplied via <see cref="IOptions{TOptions}"/> of
/// <see cref="SimulationOptions"/>, configured through the DI container.
/// </summary>
/// <remarks>
/// The constructor is <see langword="internal"/> because it takes internal types.
/// Obtain instances via the DI container (see <see cref="RuntimeServiceCollectionExtensions.AddGpssRuntime"/>).
/// </remarks>
public sealed class SimulationEngine
{
    private readonly BlockBehaviourRegistry _registry;
    private readonly IOptions<SimulationOptions> _options;
    private readonly ILogger<SimulationEngine> _logger;

    /// <summary>
    /// Initialises the engine. Use <see cref="RuntimeServiceCollectionExtensions.AddGpssRuntime"/>
    /// to obtain instances via DI.
    /// </summary>
    /// <param name="registry">Registry mapping block types to their behaviours.</param>
    /// <param name="options">Simulation run parameters resolved from the DI container.</param>
    /// <param name="logger">Logger for simulation lifecycle events.</param>
    internal SimulationEngine(
        BlockBehaviourRegistry registry,
        IOptions<SimulationOptions> options,
        ILogger<SimulationEngine> logger)
    {
        _registry = registry;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Executes <paramref name="program"/> as a discrete-event simulation and returns the result.
    /// Run parameters (termination count, random seed, event limit) are read from the
    /// <see cref="SimulationOptions"/> instance configured in the DI container.
    /// </summary>
    /// <remarks>
    /// The simulation runs until one of the following conditions is met:
    /// <list type="bullet">
    ///   <item>The termination counter reaches zero (normal end).</item>
    ///   <item>The Future Events Chain is exhausted before the counter reaches zero.</item>
    ///   <item><see cref="SimulationOptions.MaxEvents"/> is exceeded (safety limit).</item>
    /// </list>
    /// </remarks>
    /// <param name="program">The parsed GPSS model to execute.</param>
    /// <returns>A <see cref="SimulationResult"/> describing the outcome and statistics.</returns>
    public SimulationResult Run(GpssProgram program)
    {
        var options = _options.Value;

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
