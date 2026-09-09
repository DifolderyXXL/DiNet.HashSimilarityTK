using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using DiNet.HashSimilarityTK.CliToolkit.Core.Attributes;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.MetricsEngine.Application;

namespace DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;

public record GetTopFileSimilarityQuery(
    [NotFlagged] string? RootPath = null,
    int ShingleSize = 4,
    int NumHashes = 128,
    int ChunkStep = 16,
    uint Seed = 123456789,
    int Top = 5,
    string? Ignore = null) : IQuery<bool>;

internal class GetTopFileSimilarityHandler(IDocumentMatchService matchService) : IQueryHandler<GetTopFileSimilarityQuery, bool>
{
    public async Task<bool> Handle(GetTopFileSimilarityQuery query, CancellationToken ct)
{
        var context = await matchService.ExtractMatchGroupsAsync(
            query.RootPath, query.ShingleSize, query.NumHashes,
            query.ChunkStep, query.Seed, query.Ignore, ct);

        var jaccard = new JaccardSimilarityService();
        var scores = jaccard.ComputeSimilarity(context.Groups, context.Store);

        var topScores = scores.GetAllScores()
                                        .OrderByDescending(x => x.Similarity)
                                        .Take(query.Top)
                                        .ToList();

        var root = query.RootPath ?? Environment.CurrentDirectory;
        Console.WriteLine($"Top {query.Top} most similar files in: {root}");

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
    [NotFlagged] string? RootPath = null,
    int ShingleSize = 4,
    int NumHashes = 128,
    int ChunkStep = 16,
    uint Seed = 123456789,
    uint MaxBucketSizeLimit = 100,
    string? Ignore = null) : IQuery<bool>;

internal class GetLongestSequenceSimilarityHandler(IDocumentMatchService matchService) : IQueryHandler<GetLongestSequenceSimilarityQuery, bool>
{
    public async Task<bool> Handle(GetLongestSequenceSimilarityQuery query, CancellationToken ct)
    {
        var context = await matchService.ExtractMatchGroupsAsync(
            query.RootPath, query.ShingleSize, query.NumHashes,
            query.ChunkStep, query.Seed, query.Ignore, ct);


        var sequenceService = new StraightSimilarityBlockService();
        var sequence = sequenceService.ComputeSimilarity(context.Groups, query.MaxBucketSizeLimit);


        var root = query.RootPath ?? Environment.CurrentDirectory;
        Console.WriteLine($"Longest sequence in: {root}");

        if (sequence == null)
        {
            Console.WriteLine("No pairs with similarity found.");
            return true;
        }

        var linesA = context.Store.GetDocument(sequence.DocA)!.ReadLineRange(sequence.StartLineA, sequence.Length);
        var linesB = context.Store.GetDocument(sequence.DocB)!.ReadLineRange(sequence.StartLineB, sequence.Length);

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
    public static List<string> ReadLineRange(this IDocument document, long startLine, long length)
    {
        var result = new List<string>((int)length);

        using var reader = document.OpenTextReader();
        long currentLine = 0;
        string? line;
        
        while (currentLine < startLine && (line = reader.ReadLine()) != null)
        {
            currentLine++;
        }

        while (currentLine < startLine + length && (line = reader.ReadLine()) != null)
        {
            result.Add(line);
            currentLine++;
        }

        return result;
    }
}