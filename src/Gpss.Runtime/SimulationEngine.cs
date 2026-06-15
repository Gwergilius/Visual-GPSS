using Gpss.Contracts;
using Gpss.Model;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Gpss.Runtime.Internal;

namespace Gpss.Runtime;

/// <summary>
/// Executes a <see cref="GpssProgram"/> as a discrete-event simulation.
/// The simulation clock uses <see cref="double"/> time units, matching GPSS World semantics.
/// The engine is stateless: each call to <see cref="Run"/> is fully independent.
/// </summary>
public sealed class SimulationEngine
{
    /// <summary>
    /// Runs the simulation and returns the result.
    /// The simulation stops when the termination counter reaches zero,
    /// the event queue is exhausted, or <see cref="SimulationOptions.MaxEvents"/> is exceeded.
    /// </summary>
    /// <param name="program">The parsed GPSS model to execute.</param>
    /// <param name="options">Run-time parameters including the initial termination count.</param>
    /// <returns>A <see cref="SimulationResult"/> describing the outcome and statistics.</returns>
    public SimulationResult Run(GpssProgram program, SimulationOptions options)
    {
        if (options.TerminationCount <= 0)
            return BuildResult(success: true, clock: 0.0, created: 0, terminated: 0, []);

        var clock = 0.0;
        var terminationCounter = options.TerminationCount;
        var nextTxId = 1;
        var eventSeq = 0;
        var txCreated = 0L;
        var txTerminated = 0L;
        var eventsProcessed = 0L;
        var diagnostics = new List<DiagnosticMessage>();

        // Priority: (time, sequence) — sequence guarantees stable FIFO ordering for simultaneous events
        var queue = new PriorityQueue<Action, (double Time, int Seq)>();

        void Schedule(double time, Action action) =>
            queue.Enqueue(action, (time, eventSeq++));

        // Schedule the first arrival for every GENERATE block
        for (var i = 0; i < program.Blocks.Count; i++)
        {
            if (program.Blocks[i] is not GenerateBlock gen)
                continue;

            var blockIndex = i;
            var mean = Evaluate(gen.MeanInterArrivalTime);
            var firstOffset = gen.FirstTransactionOffset is not null
                ? Evaluate(gen.FirstTransactionOffset)
                : mean;
            var limit = gen.GenerationLimit is not null
                ? (long)Evaluate(gen.GenerationLimit)
                : 0L; // 0 = unlimited

            var generatedCount = 0L;

            void ScheduleArrival(double at)
            {
                if (limit > 0 && generatedCount >= limit) return;
                Schedule(at, () =>
                {
                    clock = at;
                    generatedCount++;
                    txCreated++;
                    var tx = new Transaction(nextTxId++, clock);
                    MoveTransaction(tx, blockIndex + 1);
                    ScheduleArrival(at + mean);
                });
            }

            ScheduleArrival(firstOffset);
        }

        // Main simulation loop
        while (queue.Count > 0 && terminationCounter > 0)
        {
            if (options.MaxEvents.HasValue && eventsProcessed >= options.MaxEvents.Value)
            {
                diagnostics.Add(new DiagnosticMessage(DiagnosticSeverity.Warning,
                    $"Simulation stopped after reaching the maximum event limit ({options.MaxEvents.Value})."));
                return BuildResult(success: false, clock, txCreated, txTerminated, diagnostics);
            }

            queue.Dequeue()();
            eventsProcessed++;
        }

        return BuildResult(success: true, clock, txCreated, txTerminated, diagnostics);

        // Moves a transaction through consecutive blocks until it is destroyed or the program ends.
        void MoveTransaction(Transaction tx, int startIndex)
        {
            var idx = startIndex;
            while (idx < program.Blocks.Count)
            {
                switch (program.Blocks[idx])
                {
                    case TerminateBlock term:
                        var decrement = term.DecrementCount is not null
                            ? (long)Evaluate(term.DecrementCount)
                            : 0L;
                        terminationCounter -= decrement;
                        txTerminated++;
                        return;

                    default:
                        idx++;
                        break;
                }
            }
        }
    }

    /// <summary>Evaluates a <see cref="GpssExpression"/> to a <see cref="double"/> value.</summary>
    /// <exception cref="NotSupportedException">Thrown when the expression type is not yet implemented.</exception>
    private static double Evaluate(GpssExpression expression) =>
        expression switch
        {
            IntegerExpression i => (double)i.Value,
            _ => throw new NotSupportedException(
                $"Expression type '{expression.GetType().Name}' is not supported by the runtime.")
        };

    private static SimulationResult BuildResult(
        bool success, double clock, long created, long terminated,
        List<DiagnosticMessage> diagnostics) =>
        new(success,
            new SimulationStatistics(clock, created, terminated),
            diagnostics);
}
