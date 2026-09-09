using DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;
using DiNet.HashSimilarityTK.CliToolkit.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IDocumentMatchService, DocumentMatchService>();
builder.Services.AddCliToolkit(cli =>
{
    cli.UseKebabCaseFormatter();

    cli.Handlers
        .Register<GetTopFileSimilarityHandler>("top");

    cli.Handlers
        .Register<GetLongestSequenceSimilarityHandler>("seq");
});


using var host = builder.Build();

var cliApp = host.Services.GetRequiredService<CliApplication>();
await cliApp.Route(args, CancellationToken.None);