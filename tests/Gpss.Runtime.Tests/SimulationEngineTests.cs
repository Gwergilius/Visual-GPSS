using Gpss.Contracts;
using Gpss.Model;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Gpss.Runtime;
using Shouldly;

namespace Gpss.Runtime.Tests;

public sealed class SimulationEngineTests
{
    private static readonly SimulationEngine Engine = new();

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
        var program = MinimalProgram(meanArrival: 10, decrement: 1);
        var options = new SimulationOptions(TerminationCount: terminationCount);

        var result = Engine.Run(program, options);

        result.Success.ShouldBeTrue();
        result.Statistics.SimulationEndTime.ShouldBe(expectedEndTime);
        result.Statistics.TotalTransactionsTerminated.ShouldBe(terminationCount);
        result.Statistics.TotalTransactionsCreated.ShouldBe(terminationCount);
    }

    [Theory, InlineData(0)]
    public void Run_TerminationCountZero_ReturnsImmediatelyWithNoTransactions(long terminationCount)
    {
        var program = MinimalProgram(meanArrival: 10, decrement: 1);
        var options = new SimulationOptions(TerminationCount: terminationCount);

        var result = Engine.Run(program, options);

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
        var program = MinimalProgram(meanArrival: mean, decrement: 1);
        var options = new SimulationOptions(TerminationCount: terminationCount);

        var result = Engine.Run(program, options);

        result.Statistics.SimulationEndTime.ShouldBe(expectedEndTime);
    }

    // -------------------------------------------------------------------------
    // TERMINATE decrement
    // -------------------------------------------------------------------------

    [Theory]
    [InlineData(2, 10, 5)]  // decrement=2, terminationCount=10 → 5 transactions needed
    [InlineData(5, 10, 2)]  // decrement=5, terminationCount=10 → 2 transactions needed
    public void Run_TerminateWithHigherDecrement_FewerTransactionsNeeded(
        int decrement, long terminationCount, long expectedTransactions)
    {
        var program = MinimalProgram(meanArrival: 10, decrement: decrement);
        var options = new SimulationOptions(TerminationCount: terminationCount);

        var result = Engine.Run(program, options);

        result.Success.ShouldBeTrue();
        result.Statistics.TotalTransactionsTerminated.ShouldBe(expectedTransactions);
    }

    [Theory, InlineData(10, 3)]
    public void Run_TerminateWithZeroDecrement_TransactionDestroyedButCounterUnchanged(
        int mean, long terminationCount)
    {
        // TERMINATE with no decrement operand: transaction is destroyed but counter stays at terminationCount.
        // MaxEvents prevents an infinite loop.
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(mean)),
            new TerminateBlock()
        ]);
        var options = new SimulationOptions(TerminationCount: terminationCount, MaxEvents: 20);

        var result = Engine.Run(program, options);

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
        // TERMINATE 0 means the counter never reaches zero → infinite loop without MaxEvents
        var program = new GpssProgram([
            new GenerateBlock(new IntegerExpression(10)),
            new TerminateBlock()
        ]);
        var options = new SimulationOptions(TerminationCount: 999, MaxEvents: maxEvents);

        var result = Engine.Run(program, options);

        result.Success.ShouldBeFalse();
        result.Diagnostics.ShouldContain(d => d.Severity == DiagnosticSeverity.Warning);
    }

    // -------------------------------------------------------------------------
    // Diagnostics
    // -------------------------------------------------------------------------

    [Theory, InlineData(5)]
    public void Run_NormalTermination_ProducesNoDiagnostics(long terminationCount)
    {
        var program = MinimalProgram(meanArrival: 10, decrement: 1);
        var options = new SimulationOptions(TerminationCount: terminationCount);

        var result = Engine.Run(program, options);

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
