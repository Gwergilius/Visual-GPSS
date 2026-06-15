using Gpss.Contracts;
using Gpss.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gpss.Cli;

/// <summary>Configures the DI container for the Gpss.Cli host.</summary>
public static class Startup
{
    /// <summary>
    /// Registers all services required by the CLI.
    /// Called by the Generic Host builder in <c>Program.cs</c>.
    /// </summary>
    /// <param name="services">The service collection provided by the host.</param>
    /// <param name="configuration">The host configuration (appsettings.json, environment variables, etc.).</param>
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SimulationOptions>(configuration.GetSection("Simulation"));
        services.AddGpssRuntime();
    }
}
