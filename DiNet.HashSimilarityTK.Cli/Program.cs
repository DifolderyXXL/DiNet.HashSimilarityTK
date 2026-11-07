using DiNet.CliToolkit.Services;
using DiNet.HashSimilarityTK.Cli;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


var builder = Host.CreateApplicationBuilder(args);

var logLevel = builder.Configuration.GetValue<LogLevel?>("LOG_LEVEL")
               ?? LogLevel.Error;

builder.ConfigureObservability(logLevel);
builder.Services.ConfigureBusinessServices();
builder.Services.ConfigureCli();

using var host = builder.Build();
await host.StartAsync();

var cliApp = host.Services.GetRequiredService<CliApplication>();
await cliApp.Route(args, CancellationToken.None);

await host.StopAsync();