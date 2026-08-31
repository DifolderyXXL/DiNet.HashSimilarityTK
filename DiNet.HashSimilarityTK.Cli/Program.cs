using DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;
using DiNet.HashSimilarityTK.CliToolkit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddCliToolkit(c =>
{
    c.RegisterHandler<GetTopFileSimilarityHandler>("top");
});


using var host = builder.Build();

var cliApp = host.Services.GetRequiredService<CliApplication>();
await cliApp.Route(args, CancellationToken.None);