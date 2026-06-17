using Gpss.Cli;
using Gpss.Parser;
using Gpss.Runtime;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// The first argument is the GPSS source file; remaining arguments go to the Generic Host
// (e.g. --environment Development, --Simulation:TerminationCount=10, -tc 10, --terminationCount 10)
if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: gpss.cli <file[.gpss]> [-tc|--terminationCount <n>] [--Simulation:Key=value ...]");
    return 1;
}

string filePath;
try
{
    filePath = GpssSourceFile.Resolve(args[0]);
}
catch (FileNotFoundException ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

var source = File.ReadAllText(filePath);

// Short/friendly aliases for "--Simulation:TerminationCount=<n>", which remains available as-is.
var switchMappings = new Dictionary<string, string>
{
    ["-tc"] = "Simulation:TerminationCount",
    ["--terminationCount"] = "Simulation:TerminationCount",
};

using var host = Host.CreateDefaultBuilder()
    .UseContentRoot(AppContext.BaseDirectory)
    .ConfigureAppConfiguration(config => config.AddCommandLine(args[1..], switchMappings))
    .UseSerilog((ctx, _, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
    .ConfigureServices((ctx, services) => Startup.ConfigureServices(services, ctx.Configuration))
    .Build();

var parser = host.Services.GetRequiredService<GpssParser>();

GpssParseResult parseResult;
try
{
    parseResult = parser.Parse(source, filePath);
}
catch (FileNotFoundException ex)
{
    // Thrown when an INCLUDE target (and its .gps/.gpss fallbacks) can't be found.
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}

if (!parseResult.Success)
{
    foreach (var d in parseResult.Diagnostics)
        Console.Error.WriteLine(d);
    return 1;
}

var engine = host.Services.GetRequiredService<SimulationEngine>();
var result = engine.Run(parseResult.Program!);

Console.WriteLine($"File             : {filePath}");
Console.WriteLine($"Success          : {result.Success}");
Console.WriteLine($"Simulation time  : {result.Statistics.SimulationEndTime}");
Console.WriteLine($"Transactions in  : {result.Statistics.TotalTransactionsCreated}");
Console.WriteLine($"Transactions out : {result.Statistics.TotalTransactionsTerminated}");

return result.Success ? 0 : 1;
