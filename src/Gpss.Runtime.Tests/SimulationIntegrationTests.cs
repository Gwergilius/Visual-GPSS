using Xunit;

namespace Gpss.Runtime.Tests;

/// <summary>
/// Integration tests that run complete GPSS programs and verify
/// the simulation results.
/// </summary>
public sealed class SimulationIntegrationTests
{
    // ── Simple barber-shop model ──────────────────────────────────────────────
    private const string BarberShop = """
        * Simple barber-shop model
        * Customers arrive every 18 minutes (+-6), service takes 16 min (+-4)
        GENERATE  18,6
        QUEUE     BARBERQ
        SEIZE     BARBER
        DEPART    BARBERQ
        ADVANCE   16,4
        RELEASE   BARBER
        TERMINATE 1

        START 500
        END
        """;

    [Fact]
    public void BarberShop_RunsToCompletion()
    {
        var report = GpssInterpreter.Run(BarberShop, seed: 42);
        Assert.Equal(500, report.TerminateCount);
        Assert.True(report.EndTime > 0);
    }

    [Fact]
    public void BarberShop_FacilityIsCreated()
    {
        var report = GpssInterpreter.Run(BarberShop, seed: 42);
        Assert.True(report.Facilities.ContainsKey("BARBER"));
    }

    [Fact]
    public void BarberShop_QueueIsTracked()
    {
        var report = GpssInterpreter.Run(BarberShop, seed: 42);
        Assert.True(report.Queues.ContainsKey("BARBERQ"));
        Assert.True(report.Queues["BARBERQ"].TotalEntries > 0);
    }

    [Fact]
    public void BarberShop_FacilityUtilizationInReasonableRange()
    {
        var report = GpssInterpreter.Run(BarberShop, seed: 42);
        var util   = report.Facilities["BARBER"].Utilization(report.EndTime);
        // Service rate ≈ 16/18 ≈ 0.89; utilization should be < 1
        Assert.InRange(util, 0.5, 1.0);
    }

    // ── Storage (multi-server) model ─────────────────────────────────────────
    private const string TellerModel = """
        * Bank with 3 tellers
        TELLERS STORAGE 3
        GENERATE  5,2
        QUEUE     LINE
        ENTER     TELLERS
        DEPART    LINE
        ADVANCE   12,4
        LEAVE     TELLERS
        TERMINATE 1

        START 300
        END
        """;

    [Fact]
    public void TellerModel_RunsToCompletion()
    {
        var report = GpssInterpreter.Run(TellerModel, seed: 7);
        Assert.Equal(300, report.TerminateCount);
    }

    [Fact]
    public void TellerModel_StorageCapacityIsRespected()
    {
        var report = GpssInterpreter.Run(TellerModel, seed: 7);
        Assert.True(report.Storages.ContainsKey("TELLERS"));
        Assert.Equal(3, report.Storages["TELLERS"].Capacity);
    }

    // ── Minimal model ─────────────────────────────────────────────────────────
    [Fact]
    public void MinimalModel_GenerateTerminate()
    {
        const string src = """
            GENERATE 1
            TERMINATE 1
            START 10
            END
            """;
        var report = GpssInterpreter.Run(src, seed: 1);
        Assert.Equal(10, report.TerminateCount);
    }

    // ── Parse-only API ────────────────────────────────────────────────────────
    [Fact]
    public void ParseOnly_ReturnsStatements()
    {
        const string src = "GENERATE 10\nTERMINATE 1";
        var stmts = GpssInterpreter.Parse(src);
        Assert.Equal(2, stmts.Count);
    }

    // ── Report string ─────────────────────────────────────────────────────────
    [Fact]
    public void Report_ToString_ContainsExpectedSections()
    {
        var report = GpssInterpreter.Run(BarberShop, seed: 42);
        var text   = report.ToString();
        Assert.Contains("FACILITY STATISTICS", text);
        Assert.Contains("QUEUE STATISTICS", text);
        Assert.Contains("BARBER", text);
    }
}
