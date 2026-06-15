using Gpss.Runtime.Internal;
using Gpss.Runtime.Internal.Behaviours;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Gpss.Runtime;

/// <summary>Extension methods for registering Gpss.Runtime services into a DI container.</summary>
public static class RuntimeServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Gpss.Runtime services: <see cref="SimulationEngine"/>,
    /// <see cref="IRandomNumberGenerator"/>, and all built-in <see cref="IBlockBehaviour"/> implementations.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddGpssRuntime(this IServiceCollection services)
    {
        services.AddSingleton<IRandomNumberGenerator, SystemRandomNumberGenerator>();

        // Block behaviours — register all built-in types under the internal interface
        services.AddSingleton<IBlockBehaviour, GenerateBlockBehaviour>();
        services.AddSingleton<IBlockBehaviour, TerminateBlockBehaviour>();

        // BlockBehaviourRegistry and SimulationEngine take internal types, so factory registration is required
        services.AddSingleton(sp => new BlockBehaviourRegistry(sp.GetServices<IBlockBehaviour>()));
        services.AddSingleton(sp => new SimulationEngine(
            sp.GetRequiredService<BlockBehaviourRegistry>(),
            sp.GetRequiredService<ILogger<SimulationEngine>>()));

        return services;
    }
}
