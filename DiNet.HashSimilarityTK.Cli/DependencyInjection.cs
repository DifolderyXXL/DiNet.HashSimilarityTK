using DiNet.CliToolkit.Abstraction;
using DiNet.CliToolkit.Services;
using DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;
using DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;
using DiNet.HashSimilarityTK.Cli.Services.Telemetry;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace DiNet.HashSimilarityTK.Cli;

public static class DependencyInjection
{
    private static readonly Lock ConsoleLock = new();
    public static void ConfigureObservability(this HostApplicationBuilder builder, LogLevel logLevel)
    {
        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(logLevel);

        if (logLevel != LogLevel.None)
        {
            builder.Logging.AddConsole();
            builder.Logging.AddFilter("Microsoft.Hosting", LogLevel.Warning);
            builder.Logging.AddFilter("Microsoft.Extensions.Hosting", LogLevel.Warning);
        }

        builder.Services.AddSingleton(Diagnostics.Source);
        builder.Services.AddSingleton<ITelemetry, Telemetry>();

        if(logLevel <= LogLevel.Debug)
        {
            ActivitySource.AddActivityListener(new ActivityListener
            {
                ShouldListenTo = source => source.Name == Diagnostics.Source.Name,
                Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
                ActivityStopped = activity =>
                {
                    var ms = activity.Duration.TotalMilliseconds;
                    var isError = activity.Status == ActivityStatusCode.Error;

                    lock (ConsoleLock)
                    {
                        Console.ForegroundColor = isError ? ConsoleColor.Red : ConsoleColor.DarkGray;

                        Console.Error.WriteLine($"[trace] {activity.DisplayName} — {ms:F1} ms {(isError ? "[FAILED]" : "")}");
                        Console.ResetColor();
                    }

                }
            });
        }
    }

    public static void ConfigureBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IDocumentMatchService, DocumentMatchService>();
        services.AddSingleton(new LineFilterBuilder()
            .IgnoreWhitespace()
            .MinLength(3)
            .IgnoreStructuralSymbols()
            .IgnoreComments()
            .IgnoreImports()
            .Build());

        services.AddSingleton<DocumentReader>();
        services.AddSingleton<IDocumentReader>(sp => sp.GetRequiredService<DocumentReader>());
        services.Decorate<IDocumentReader, FilteredDocumentReader>();
    }

    public static void ConfigureCli(this IServiceCollection services)
    {
        services.AddCliToolkit(cli =>
        {
            cli.UseKebabCaseFormatter();
            cli.Handlers
                .IntroduceHelpCommand("help")
                .Register<GetTopFileSimilarityHandler>("top")
                .Register<GetLongestSequenceSimilarityHandler>("seq")
                .Register<IndexingStatisticsHandler>("index");
        });

        services.AddSingleton<IQueryHandlerResolver>(sp =>
            new TelemetryQueryHandlerResolver(
                new DefaultQueryHandlerResolver(sp),
                sp.GetRequiredService<ITelemetry>()));
    }
}
