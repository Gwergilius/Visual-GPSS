using Gpss.Cli;
using Gpss.Parser;
using Gpss.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, _, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
    .ConfigureServices((ctx, services) => Startup.ConfigureServices(services, ctx.Configuration))
    .Build();

const string source = """
    GENERATE 10
    TERMINATE 1
    """;

var parser = host.Services.GetRequiredService<GpssParser>();
var parseResult = parser.Parse(source);

if (!parseResult.Success)
{
    foreach (var d in parseResult.Diagnostics)
        Console.Error.WriteLine($"[{d.Severity}] {d.Message}");
    return;
}

var engine = host.Services.GetRequiredService<SimulationEngine>();
var result = engine.Run(parseResult.Program!);

Console.WriteLine();
Console.WriteLine($"Success          : {result.Success}");
Console.WriteLine($"Simulation time  : {result.Statistics.SimulationEndTime}");
Console.WriteLine($"Transactions in  : {result.Statistics.TotalTransactionsCreated}");
Console.WriteLine($"Transactions out : {result.Statistics.TotalTransactionsTerminated}");
