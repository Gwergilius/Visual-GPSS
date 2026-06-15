using Gpss.Runtime.Simulation;
using Xunit;

namespace Gpss.Runtime.Tests;

public sealed class SimulationEntityTests
{
    [Fact]
    public void Facility_TrySeize_WhenFree_ReturnsTrue()
    {
        var fac  = new Facility("FAC1");
        var xact = new Transaction();
        Assert.True(fac.TrySeize(xact, 0));
        Assert.True(fac.IsSeized);
        Assert.Equal(xact, fac.Holder);
    }

    [Fact]
    public void Facility_TrySeize_WhenBusy_ReturnsFalse()
    {
        var fac   = new Facility("FAC1");
        var xact1 = new Transaction();
        var xact2 = new Transaction();
        fac.TrySeize(xact1, 0);
        Assert.False(fac.TrySeize(xact2, 0));
    }

    [Fact]
    public void Facility_Release_FreesHolder()
    {
        var fac  = new Facility("FAC1");
        var x    = new Transaction();
        fac.TrySeize(x, 0);
        fac.Release(x, 10);
        Assert.False(fac.IsSeized);
        Assert.Equal(10.0, fac.TotalBusyTime);
    }

    [Fact]
    public void Facility_Release_ReturnsNextWaiting()
    {
        var fac  = new Facility("FAC1");
        var x1   = new Transaction();
        var x2   = new Transaction();
        fac.TrySeize(x1, 0); // x1 holds
        fac.TrySeize(x2, 0); // x2 waits
        var released = fac.Release(x1, 5);
        Assert.Equal(x2, released);
        Assert.Equal(x2, fac.Holder);
    }

    [Fact]
    public void Storage_TryEnter_WhenCapacityAvailable()
    {
        var s = new Storage("STOR", 3);
        var x = new Transaction();
        Assert.True(s.TryEnter(x, 0));
        Assert.Equal(1, s.InUse);
        Assert.Equal(2, s.Available);
    }

    [Fact]
    public void Storage_TryEnter_WhenFull_ReturnsFalse()
    {
        var s = new Storage("STOR", 1);
        var x1 = new Transaction();
        var x2 = new Transaction();
        s.TryEnter(x1, 0);
        Assert.False(s.TryEnter(x2, 0));
    }

    [Fact]
    public void Storage_Leave_DecrementsInUse()
    {
        var s = new Storage("STOR", 2);
        var x = new Transaction();
        s.TryEnter(x, 0);
        s.Leave(5);
        Assert.Equal(0, s.InUse);
    }

    [Fact]
    public void Queue_Enter_And_Depart_TracksWaitTime()
    {
        var q = new QueueEntity("Q1");
        var x = new Transaction();
        q.Enter(x, 0);
        Assert.Equal(1, q.CurrentCount);
        q.Depart(x, 10);
        Assert.Equal(0, q.CurrentCount);
        Assert.Equal(10.0, q.TotalWait);
        Assert.Equal(10.0, q.AverageTime);
    }

    [Fact]
    public void Queue_ZeroWaitEntry_IsTracked()
    {
        var q = new QueueEntity("Q1");
        var x = new Transaction();
        q.Enter(x, 5);
        q.Depart(x, 5); // zero wait
        Assert.Equal(1L, q.ZeroEntries);
        Assert.Equal(0.0, q.AverageTimeNonZero);
    }

    [Fact]
    public void Table_Record_UpdatesStatistics()
    {
        var t = new TableEntity("TAB1", 0, 10, 5);
        t.Record(5);
        t.Record(15);
        t.Record(25);
        Assert.Equal(3L, t.TotalEntries);
        Assert.Equal(15.0, t.Mean);
        Assert.True(t.StdDev > 0);
    }
}
