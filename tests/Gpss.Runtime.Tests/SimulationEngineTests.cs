using Gpss.Contracts;
using Gpss.Model;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Gpss.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Gpss.Runtime.Tests;

public sealed class SimulationEngineTests
{
    private static SimulationEngine CreateEngine(
        long terminationCount = 1,
        long? maxEvents = null)
    {
        var services = new ServiceCollection()
            .AddLogging(b => b.SetMinimumLevel(LogLevel.None))
            .Configure<SimulationOptions>(o =>
            {
                o.TerminationCount = terminationCount;
                o.MaxEvents = maxEvents;
            })
            .AddGpssRuntime()
            .BuildServiceProvider();
        return services.GetRequiredService<SimulationEngine>();
    }

    // -------------------------------------------------------------------------
    // Termination counter
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1,  10.0)]
    [InlineData(5,  50.0)]
    [InlineData(10, 100.0)]
    public void Run_GenerateTerminate_StopsWhenTerminationCounterReachesZero(
        long terminationCount, double expectedEndTime)
    {
        var result = CreateEngine(terminationCount).Run(MinimalProgram(meanArrival: 10, decrement: 1));

        result.Success.ShouldBeTrue();
        result.Statistics.SimulationEndTime.ShouldBe(expectedEndTime);
        result.Statistics.TotalTransactionsTerminated.ShouldBe(terminationCount);
        result.Statistics.TotalTransactionsCreated.ShouldBe(terminationCount);
    }

    [Theory, InlineData(0)]
    public void Run_TerminationCountZero_ReturnsImmediatelyWithNoTransactions(long terminationCount)
    {
        var result = CreateEngine(terminationCount).Run(MinimalProgram(meanArrival: 10, decrement: 1));

        result.Success.ShouldBeTrue();
        result.Statistics.SimulationEndTime.ShouldBe(0.0);
        result.Statistics.TotalTransactionsCreated.ShouldBe(0);
        result.Statistics.TotalTransactionsTerminated.ShouldBe(0);
    }

    // -------------------------------------------------------------------------
    // Clock
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(10, 5,  50.0)]
    [InlineData(25, 4, 100.0)]
    [InlineData(1,  1,   1.0)]
    public void Run_SimulationClock_AdvancesByMeanInterArrivalTime(
        int mean, long terminationCount, double expectedEndTime)
    {
        var result = CreateEngine(terminationCount).Run(MinimalProgram(meanArrival: mean, decrement: 1));

        result.Statistics.SimulationEndTime.ShouldBe(expectedEndTime);
    }

    // -------------------------------------------------------------------------
    // TERMINATE decrement
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(2, 10, 5)]
    [InlineData(5, 10, 2)]
    public void Run_TerminateWithHigherDecrement_FewerTransactionsNeeded(
        int decrement, long terminationCount, long expectedTransactions)
    {
        var result = CreateEngine(terminationCount).Run(MinimalProgram(meanArrival: 10, decrement: decrement));

        result.Success.ShouldBeTrue();
        result.Statistics.TotalTransactionsTerminated.ShouldBe(expectedTransactions);
    }

    [Theory, InlineData(10, 3)]
    public void Run_TerminateWithZeroDecrement_TransactionDestroyedButCounterUnchanged(
        int mean, long terminationCount)
    {
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(mean)),
            new TerminateBlock()
        ]);

        var result = CreateEngine(terminationCount, maxEvents: 20).Run(program);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Warning);
        result.Statistics.TotalTransactionsCreated.ShouldBeGreaterThan(0);
    }

    // -------------------------------------------------------------------------
    // MaxEvents safety limit
    // -------------------------------------------------------------------------

    [Theory, InlineData(5)]
    public void Run_MaxEventsExceeded_ReturnsFalseWithWarning(long maxEvents)
    {
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(10)),
            new TerminateBlock()
        ]);

        var result = CreateEngine(terminationCount: 999, maxEvents: maxEvents).Run(program);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Warning);
    }

    // -------------------------------------------------------------------------
    // Diagnostics
    // -------------------------------------------------------------------------

    [Theory, InlineData(5)]
    public void Run_NormalTermination_ProducesNoDiagnostics(long terminationCount)
    {
        var result = CreateEngine(terminationCount).Run(MinimalProgram(meanArrival: 10, decrement: 1));

        result.Diagnostics.ShouldBeEmpty();
    }

    // -------------------------------------------------------------------------
    // ADVANCE
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1, 50.0)]
    [InlineData(3, 70.0)]
    [InlineData(5, 90.0)]
    public void Run_Advance_DelaysTransactionBeforeContinuing(
        long terminationCount, double expectedEndTime)
    {
        // GENERATE (mean=10) → ADVANCE (delay=40) → TERMINATE: the N-th transaction
        // terminates at N*10 + 40
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(10)),
            new AdvanceBlock(new IntegerExpression(40)),
            new TerminateBlock(new IntegerExpression(1))
        ]);

        var result = CreateEngine(terminationCount).Run(program);

        result.Success.ShouldBeTrue();
        result.Statistics.SimulationEndTime.ShouldBe(expectedEndTime);
        result.Statistics.TotalTransactionsTerminated.ShouldBe(terminationCount);
    }

    [Theory, InlineData(1)]
    public void Run_AdvanceWithoutOperand_NoDelayIsApplied(long terminationCount)
    {
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(10)),
            new AdvanceBlock(),
            new TerminateBlock(new IntegerExpression(1))
        ]);

        var result = CreateEngine(terminationCount).Run(program);

        result.Success.ShouldBeTrue();
        result.Statistics.SimulationEndTime.ShouldBe(10.0);
    }

    // -------------------------------------------------------------------------
    // QUEUE / DEPART (statistical blocks — no flow delay)
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1, 10.0)]
    [InlineData(3, 30.0)]
    public void Run_QueueDepart_TransactionsFlowThroughWithoutDelay(
        long terminationCount, double expectedEndTime)
    {
        // QUEUE and DEPART are statistical-only; the transaction must not be delayed
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(10)),
            new QueueBlock(new SymbolExpression("Server")),
            new DepartBlock(new SymbolExpression("Server")),
            new TerminateBlock(new IntegerExpression(1))
        ]);

        var result = CreateEngine(terminationCount).Run(program);

        result.Success.ShouldBeTrue();
        result.Statistics.SimulationEndTime.ShouldBe(expectedEndTime);
        result.Statistics.TotalTransactionsTerminated.ShouldBe(terminationCount);
    }

    // -------------------------------------------------------------------------
    // SEIZE / RELEASE
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(1, 10.0)]
    [InlineData(3, 30.0)]
    public void Run_SeizeRelease_TransactionsFlowThroughUncontestedFacility(
        long terminationCount, double expectedEndTime)
    {
        // GENERATE → SEIZE → RELEASE → TERMINATE (no queuing: each tx arrives after previous is done)
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(10)),
            new SeizeBlock(new SymbolExpression("Server")),
            new ReleaseBlock(new SymbolExpression("Server")),
            new TerminateBlock(new IntegerExpression(1))
        ]);

        var result = CreateEngine(terminationCount).Run(program);

        result.Success.ShouldBeTrue();
        result.Statistics.SimulationEndTime.ShouldBe(expectedEndTime);
        result.Statistics.TotalTransactionsTerminated.ShouldBe(terminationCount);
    }

    [Theory, InlineData(3)]
    public void Run_SeizeRelease_ProducesNoDiagnosticsForUncontestedFacility(long terminationCount)
    {
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(10)),
            new SeizeBlock(new SymbolExpression("Server")),
            new ReleaseBlock(new SymbolExpression("Server")),
            new TerminateBlock(new IntegerExpression(1))
        ]);

        var result = CreateEngine(terminationCount).Run(program);

        result.Diagnostics.ShouldBeEmpty();
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private static GpssProgram MinimalProgram(int meanArrival, int decrement) =>
        new([
            new GenerateBlock(new IntegerExpression(meanArrival)),
            new TerminateBlock(new IntegerExpression(decrement))
        ]);
}
