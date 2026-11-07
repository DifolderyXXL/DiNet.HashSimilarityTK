using DiNet.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.Cli.Handlers.Common;
using DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;
using DiNet.HashSimilarityTK.Cli.Services.Telemetry;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.MetricsEngine.Application;

namespace DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;

public record GetTopFileSimilarityQuery(
    int Top = 5) : LSHBaseQuery;

internal class GetTopFileSimilarityHandler(IDocumentMatchService matchService, ITelemetry telemetry) : IQueryHandler<GetTopFileSimilarityQuery, bool>
{
    public async Task<bool> Handle(GetTopFileSimilarityQuery query, CancellationToken ct)
    {
        var store = telemetry.Process("Index", () => RepositoryIndexHelper.Index(query));

        var context = await matchService.ExtractMatchGroupsAsync(store,
            query.ShingleSize, query.NumHashes,
            query.ChunkStep, query.Seed, ct);


        var topScores = telemetry.Process(
                    "Compute similarity",
                    (context, query.Top),
                    static state =>
                    {
                        var jaccard = new JaccardSimilarityService();
                        var scores = jaccard.ComputeSimilarity(state.context.Groups, state.context.Store);

                        return scores.GetAllScores()
                            .OrderByDescending(x => x.Similarity)
                            .Take(state.Top)
                            .ToList();
                    });

        Console.WriteLine($"Top {query.Top} most similar files in: {query.Root}");

        if (!topScores.Any())
        {
            Console.WriteLine("No pairs with similarity found.");
            return true;
        }

        foreach (var score in topScores)
        {
            var fileA = context.Store.GetDocument(score.FileA)?.FullPath ?? "Unknown";
            var fileB = context.Store.GetDocument(score.FileB)?.FullPath ?? "Unknown";
            Console.WriteLine($"{fileA} <-> {fileB} : {score.Similarity * 100:F2}%");
        }

        return true;
    }
}

public static class DocumentHelper
{
    public static async Task<List<string>> ReadLineRange(this IAsyncEnumerable<LineData> lines, int startLine, int length)
        => await lines.Skip(startLine).Take(length).Select(x => x.Text).ToListAsync();

}