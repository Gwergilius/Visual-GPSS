using Gpss.Contracts;
using Gpss.Runtime.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Gpss.Runtime;

/// <summary>Extension methods for registering Gpss.Runtime services into a DI container.</summary>
public static class RuntimeServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Gpss.Runtime services: <see cref="SimulationEngine"/>,
    /// <see cref="IRandomNumberGeneratorFactory"/>, and all <see cref="IBlockBehaviour"/> implementations
    /// discovered automatically via reflection in the <c>Gpss.Runtime</c> assembly.
    /// </summary>
    /// <remarks>
    /// Block behaviours are discovered by scanning the assembly for all concrete, non-abstract
    /// classes that implement <see cref="IBlockBehaviour"/>. Adding a new block behaviour class
    /// to the assembly is sufficient — no manual registration is required.
    /// </remarks>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddGpssRuntime(this IServiceCollection services)
    {
        services.AddSingleton<IRandomNumberGeneratorFactory>(sp =>
        {
            var seed = sp.GetRequiredService<IOptions<SimulationOptions>>().Value.RandomSeed;
            return seed is { } s ? new SystemRandomNumberGeneratorFactory(s) : new SystemRandomNumberGeneratorFactory();
        });

        // Auto-discover and register all IBlockBehaviour implementations in this assembly
        foreach (var type in DiscoverBlockBehaviours())
            services.AddSingleton(typeof(IBlockBehaviour), type);

        // BlockBehaviourRegistry and SimulationEngine take internal types, so factory registration is required
        services.AddSingleton(sp => new BlockBehaviourRegistry(sp.GetServices<IBlockBehaviour>()));
        services.AddSingleton(sp => new SimulationEngine(
            sp.GetRequiredService<BlockBehaviourRegistry>(),
            sp.GetRequiredService<IOptions<SimulationOptions>>(),
            sp.GetRequiredService<ILogger<SimulationEngine>>()));

        return services;
    }

    /// <summary>
    /// Returns all concrete <see cref="IBlockBehaviour"/> implementation types defined in the
    /// <c>Gpss.Runtime</c> assembly, ordered by name for deterministic registration.
    /// </summary>
    private static IEnumerable<Type> DiscoverBlockBehaviours() =>
        typeof(RuntimeServiceCollectionExtensions).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                     && t.IsAssignableTo(typeof(IBlockBehaviour)))
            .OrderBy(t => t.Name);
}
