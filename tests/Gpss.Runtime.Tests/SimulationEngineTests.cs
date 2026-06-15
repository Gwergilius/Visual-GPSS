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
    private static SimulationEngine CreateEngine()
    {
        var services = new ServiceCollection()
            .AddLogging(b => b.SetMinimumLevel(LogLevel.None))
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
        var result = CreateEngine().Run(MinimalProgram(meanArrival: 10, decrement: 1),
            new SimulationOptions(TerminationCount: terminationCount));

        result.Success.ShouldBeTrue();
        result.Statistics.SimulationEndTime.ShouldBe(expectedEndTime);
        result.Statistics.TotalTransactionsTerminated.ShouldBe(terminationCount);
        result.Statistics.TotalTransactionsCreated.ShouldBe(terminationCount);
    }

    [Theory, InlineData(0)]
    public void Run_TerminationCountZero_ReturnsImmediatelyWithNoTransactions(long terminationCount)
    {
        var result = CreateEngine().Run(MinimalProgram(meanArrival: 10, decrement: 1),
            new SimulationOptions(TerminationCount: terminationCount));

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
        var result = CreateEngine().Run(MinimalProgram(meanArrival: mean, decrement: 1),
            new SimulationOptions(TerminationCount: terminationCount));

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
        var result = CreateEngine().Run(MinimalProgram(meanArrival: 10, decrement: decrement),
            new SimulationOptions(TerminationCount: terminationCount));

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

        var result = CreateEngine().Run(program,
            new SimulationOptions(TerminationCount: terminationCount, MaxEvents: 20));

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

        var result = CreateEngine().Run(program,
            new SimulationOptions(TerminationCount: 999, MaxEvents: maxEvents));

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Warning);
    }

    // -------------------------------------------------------------------------
    // Diagnostics
    // -------------------------------------------------------------------------

    [Theory, InlineData(5)]
    public void Run_NormalTermination_ProducesNoDiagnostics(long terminationCount)
    {
        var result = CreateEngine().Run(MinimalProgram(meanArrival: 10, decrement: 1),
            new SimulationOptions(TerminationCount: terminationCount));

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
