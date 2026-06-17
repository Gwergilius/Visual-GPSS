using Gpss.Parser.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Gpss.Parser;

/// <summary>Extension methods for registering Gpss.Parser services into a DI container.</summary>
public static class ParserServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="GpssParser"/> and all <see cref="IBlockBuilder"/> implementations
    /// discovered automatically via reflection in the <c>Gpss.Parser</c> assembly.
    /// </summary>
    /// <remarks>
    /// Block builders are discovered by scanning the assembly for all concrete, non-abstract
    /// classes that implement <see cref="IBlockBuilder"/>. Adding a new block builder class
    /// to the assembly is sufficient — no manual registration is required.
    /// </remarks>
    /// <param name="services">The service collection to register into.</param>
    /// <returns>The same <paramref name="services"/> for chaining.</returns>
    public static IServiceCollection AddGpssParser(this IServiceCollection services)
    {
        // Auto-discover and register all IBlockBuilder implementations in this assembly
        foreach (var type in DiscoverBlockBuilders())
            services.AddSingleton(typeof(IBlockBuilder), type);

        // BlockBuilderRegistry and GpssParser take internal types, so factory registration is required
        services.AddSingleton(sp => new BlockBuilderRegistry(sp.GetServices<IBlockBuilder>()));
        services.AddSingleton(sp => new GpssParser(sp.GetRequiredService<BlockBuilderRegistry>()));

        return services;
    }

    /// <summary>
    /// Returns all concrete <see cref="IBlockBuilder"/> implementation types defined in the
    /// <c>Gpss.Parser</c> assembly, ordered by name for deterministic registration.
    /// </summary>
    private static IEnumerable<Type> DiscoverBlockBuilders() =>
        typeof(ParserServiceCollectionExtensions).Assembly
            .GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false }
                     && t.IsAssignableTo(typeof(IBlockBuilder)))
            .OrderBy(t => t.Name);
}
