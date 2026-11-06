using DiNet.CliToolkit.Services;
using DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;
using DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddScoped<IDocumentMatchService, DocumentMatchService>();

builder.Services.AddSingleton(
    new LineFilterBuilder()
            .IgnoreWhitespace()
            .MinLength(3)
            .IgnoreStructuralSymbols()
            .IgnoreComments()
            .IgnoreImports()
            .Build()
);

builder.Services.AddSingleton<DocumentReader>();

builder.Services.AddSingleton<IDocumentReader>(sp => sp.GetRequiredService<DocumentReader>());
builder.Services.Decorate<IDocumentReader, FilteredDocumentReader>();

builder.Services.AddCliToolkit(cli =>
{
    cli.UseKebabCaseFormatter();

    cli.Handlers
        .IntroduceHelpCommand("help")
        .Register<GetTopFileSimilarityHandler>("top");

    cli.Handlers
        .Register<GetLongestSequenceSimilarityHandler>("seq");


    cli.Handlers
        .Register<IndexingStatisticsHandler>("index");
});


using var host = builder.Build();

var cliApp = host.Services.GetRequiredService<CliApplication>();
await cliApp.Route(args, CancellationToken.None);