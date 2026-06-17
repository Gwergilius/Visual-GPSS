using Gpss.Cli;
using Gpss.Parser;
using Gpss.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

// The first argument is the GPSS source file; remaining arguments go to the Generic Host
// (e.g. --environment Development, --Simulation:TerminationCount=10)
if (args.Length == 0)
{
    Console.Error.WriteLine("Usage: gpss.cli <file[.gpss]>");
    return 1;
}

var filePath = args[0];
if (!Path.HasExtension(filePath))
    filePath += ".gpss";

if (!File.Exists(filePath))
{
    Console.Error.WriteLine($"Error: file not found: '{filePath}'");
    return 1;
}

var source = File.ReadAllText(filePath);

using var host = Host.CreateDefaultBuilder(args[1..])
    .UseContentRoot(AppContext.BaseDirectory)
    .UseSerilog((ctx, _, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
    .ConfigureServices((ctx, services) => Startup.ConfigureServices(services, ctx.Configuration))
    .Build();

var parser = host.Services.GetRequiredService<GpssParser>();
var parseResult = parser.Parse(source);

if (!parseResult.Success)
{
    foreach (var d in parseResult.Diagnostics)
        Console.Error.WriteLine($"[{d.Severity}] {d.Message}");
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
