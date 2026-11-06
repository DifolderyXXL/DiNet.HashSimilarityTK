using DiNet.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.Cli.Handlers.RepositoryStatistics;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.MetricsEngine.Application;

namespace DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;

public record GetTopFileSimilarityQuery(
    int ShingleSize = 4,
    int NumHashes = 128,
    int ChunkStep = 16,
    uint Seed = 123456789,
    int Top = 5) : RootBaseQuery;

internal class GetTopFileSimilarityHandler(IDocumentMatchService matchService) : IQueryHandler<GetTopFileSimilarityQuery, bool>
{
    public async Task<bool> Handle(GetTopFileSimilarityQuery query, CancellationToken ct)
    {
        var store = RepositoryIndexHelper.Index(query);

        var context = await matchService.ExtractMatchGroupsAsync(store,
            query.ShingleSize, query.NumHashes,
            query.ChunkStep, query.Seed, ct);

        var jaccard = new JaccardSimilarityService();
        var scores = jaccard.ComputeSimilarity(context.Groups, context.Store);

        var topScores = scores.GetAllScores()
                                        .OrderByDescending(x => x.Similarity)
                                        .Take(query.Top)
                                        .ToList();

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


public record GetLongestSequenceSimilarityQuery(
    int ShingleSize = 4,
    int NumHashes = 128,
    int ChunkStep = 16,
    uint Seed = 123456789,
    uint MaxBucketSizeLimit = 100,
    bool CompareDifferentDocuments = false) : RootBaseQuery;

internal class GetLongestSequenceSimilarityHandler(IDocumentMatchService matchService, DocumentReader reader) : IQueryHandler<GetLongestSequenceSimilarityQuery, bool>
{
    public async Task<bool> Handle(GetLongestSequenceSimilarityQuery query, CancellationToken ct)
    {
        var store = RepositoryIndexHelper.Index(query);

        var context = await matchService.ExtractMatchGroupsAsync(store,
            query.ShingleSize, query.NumHashes,
            query.ChunkStep, query.Seed, ct);

        var sequenceService = new StraightSimilarityBlockService();
        var sequence = sequenceService.ComputeSimilarity(context.Groups, query.MaxBucketSizeLimit, query.CompareDifferentDocuments);


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

public static class DocumentHelper
{
    public static async Task<List<string>> ReadLineRange(this IAsyncEnumerable<LineData> lines, int startLine, int length)
        => await lines.Skip(startLine).Take(length).Select(x => x.Text).ToListAsync();

}