using Gpss.Cli;
using Gpss.Model;
using Gpss.Model.Blocks;
using Gpss.Model.Expressions;
using Gpss.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = Host.CreateDefaultBuilder(args)
    .UseSerilog((ctx, _, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration))
    .ConfigureServices((ctx, services) => Startup.ConfigureServices(services, ctx.Configuration))
    .Build();

// Temporary demo: hardcoded GENERATE 10 / TERMINATE 1 model
// TerminationCount is read from appsettings.json → "Simulation:TerminationCount"
var engine = host.Services.GetRequiredService<SimulationEngine>();

var program = new GpssProgram([
    new GenerateBlock(new IntegerExpression(10)),
    new TerminateBlock(new IntegerExpression(1))
]);

var result = engine.Run(program);

Console.WriteLine();
Console.WriteLine($"Success          : {result.Success}");
Console.WriteLine($"Simulation time  : {result.Statistics.SimulationEndTime}");
Console.WriteLine($"Transactions in  : {result.Statistics.TotalTransactionsCreated}");
Console.WriteLine($"Transactions out : {result.Statistics.TotalTransactionsTerminated}");
