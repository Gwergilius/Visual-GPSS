using Gpss.Contracts;
using Gpss.Model.Blocks;
using Gpss.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace Gpss.Runtime.Tests;

public sealed class BlockBehaviourRegistrySyncTests
{
    private static IServiceCollection CreateServices() =>
        new ServiceCollection()
            .AddLogging(b => b.SetMinimumLevel(LogLevel.None))
            .Configure<SimulationOptions>(o => o.TerminationCount = 1)
            .AddGpssRuntime();

    [Fact]
    public void AddGpssRuntime_AllKnownBlocks_ResolveWithoutError()
    {
        var provider = CreateServices().BuildServiceProvider();

        Should.NotThrow(() => provider.GetRequiredService<SimulationEngine>());
    }

    [Fact]
    public void AddGpssRuntime_KnownBlockWithoutRegisteredBehaviour_ThrowsOnResolve()
    {
        // IBlockBehaviour and its implementations are internal to Gpss.Runtime, so the
        // matching descriptor is located by reflection metadata rather than typeof(...).
        var services = CreateServices();
        var missingBehaviour = services.First(d =>
            d.ServiceType.Name == "IBlockBehaviour" &&
            d.ImplementationType?.Name == "GenerateBlockBehaviour");
        services.Remove(missingBehaviour);

        var provider = services.BuildServiceProvider();

        var ex = Should.Throw<InvalidOperationException>(() => provider.GetRequiredService<SimulationEngine>());
        ex.Message.ShouldContain(nameof(GenerateBlock));
    }
}
