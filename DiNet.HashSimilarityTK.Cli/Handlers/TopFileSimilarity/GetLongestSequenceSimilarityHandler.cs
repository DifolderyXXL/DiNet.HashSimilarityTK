using DiNet.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.Cli.Handlers.Common;
using DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;
using DiNet.HashSimilarityTK.Cli.Services.Telemetry;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.MetricsEngine.Application;

namespace DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;

public record GetLongestSequenceSimilarityQuery(
    uint MaxBucketSizeLimit = 100,
    bool CompareDifferentDocuments = false) : LSHBaseQuery;

internal class GetLongestSequenceSimilarityHandler(
    IDocumentMatchService matchService,
    DocumentReader reader,
    ITelemetry telemetry) : IQueryHandler<GetLongestSequenceSimilarityQuery, bool>
{
    public async Task<bool> Handle(GetLongestSequenceSimilarityQuery query, CancellationToken ct)
    {
        var store = telemetry.Process("Index", () => RepositoryIndexHelper.Index(query));

        var context = await matchService.ExtractMatchGroupsAsync(store,
            query.ShingleSize, query.NumHashes,
            query.ChunkStep, query.Seed, ct);

        var sequenceService = new StraightSimilarityBlockService();

        var sequence = telemetry.Process("Compute similarity",
            () => sequenceService.ComputeSimilarity(context.Groups, query.MaxBucketSizeLimit, query.CompareDifferentDocuments));

        Console.WriteLine($"Longest sequence in: {query.Root}");

        if (sequence == null)
        {
            Console.WriteLine("No pairs with similarity found.");
            return true;
        }


        var linesA = await reader.EnumerateLines(context.Store.GetDocument(sequence.DocA)!, ct)
            .ReadLineRange((int)sequence.StartLineA, (int)sequence.Length);

        var linesB = await reader.EnumerateLines(context.Store.GetDocument(sequence.DocB)!, ct)
            .ReadLineRange((int)sequence.StartLineB, (int)sequence.Length);

        var fileA = context.Store.GetDocument(sequence.DocA)?.FullPath ?? "Unknown";
        var fileB = context.Store.GetDocument(sequence.DocB)?.FullPath ?? "Unknown";

        Console.WriteLine($"Longest sequence is {sequence.Length} lines:");
        Console.WriteLine($"  File A: {fileA} (lines {sequence.StartLineA}..{sequence.StartLineA + sequence.Length - 1})");
        Console.WriteLine($"  File B: {fileB} (lines {sequence.StartLineB}..{sequence.StartLineB + sequence.Length - 1})");

        for (int i = 0; i < sequence.Length; i++)
        {
            var lineNumA = sequence.StartLineA + i;
            var lineNumB = sequence.StartLineB + i;

            var textA = i < linesA.Count ? linesA[i] : "<EOF>";
            var textB = i < linesB.Count ? linesB[i] : "<EOF>";

            if (textA == textB)
            {
                Console.WriteLine($"[{lineNumA,4} | {lineNumB,4}] {textA}");
            }
            else
            {
                Console.WriteLine($"[{lineNumA,4} A] {textA}");
                Console.WriteLine($"[{lineNumB,4} B] {textB}");
                Console.WriteLine(new string('-', 40));
            }
        }

        return true;
    }
}
