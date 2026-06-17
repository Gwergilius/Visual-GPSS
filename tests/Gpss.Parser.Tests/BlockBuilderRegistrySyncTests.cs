using Gpss.Model.Blocks;
using Gpss.Parser;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Gpss.Parser.Tests;

public sealed class BlockBuilderRegistrySyncTests
{
    private static IServiceCollection CreateServices() => new ServiceCollection().AddGpssParser();

    [Fact]
    public void AddGpssParser_AllKnownBlocks_ResolveWithoutError()
    {
        var provider = CreateServices().BuildServiceProvider();

        Should.NotThrow(() => provider.GetRequiredService<GpssParser>());
    }

    [Fact]
    public void AddGpssParser_KnownBlockWithoutRegisteredBuilder_ThrowsOnResolve()
    {
        // IBlockBuilder and its implementations are internal to Gpss.Parser, so the
        // matching descriptor is located by reflection metadata rather than typeof(...).
        var services = CreateServices();
        var missingBuilder = services.First(d =>
            d.ServiceType.Name == "IBlockBuilder" &&
            d.ImplementationType?.Name == "GenerateBlockBuilder");
        services.Remove(missingBuilder);

        var provider = services.BuildServiceProvider();

        var ex = Should.Throw<InvalidOperationException>(() => provider.GetRequiredService<GpssParser>());
        ex.Message.ShouldContain(nameof(GenerateBlock));
    }
}
