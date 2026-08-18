using DiNet.HashSimilarityTK.Cli;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.Infrastructure;
using DiNet.HashSimilarityTK.MetricsEngine.Application;
using DiNet.HashSimilarityTK.MetricsEngine.Infrastructure;


var cli = CliArgumentHelper.Create(args);

var rootPath = cli.GetPositional(0);
if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
{
    Console.WriteLine("Usage: hsim <root-path> [--top <n>] [--shingleSize <n>] [--numHashes <n>] [--chunkStep <n>] [--seed <n>]");
    return 1;
}

var top = cli.GetInt("--top", 5);
var shingleSize = cli.GetInt("--shingleSize", 4);
var numHashes = cli.GetInt("--numHashes", 128);
var chunkStep = cli.GetInt("--chunkStep", 16);
var seed = cli.GetInt("--seed", 123456789);
var ignore = cli.GetFlagArgument("--ignore");


try
{


    var matchTable = new MatchSet<DocumentFileLine>(
        shingleSize: shingleSize,
        numHashes: numHashes,
        chunkStep: chunkStep,
        seed: (uint)seed
    );


    var fileSystem = new GitIgnoredFileSystem(ignore);
    var indexer = new DocumentIndexer(fileSystem);

    var store = indexer.BuildIndex(rootPath);


    var processingService = new DocumentProcessingService(
        store,
        new XxHasher((uint)seed),
        matchTable,
        (document, hash, line) => new DocumentFileLine(document.Id, line, hash)
    );

    await processingService.ProcessAllDocumentsAsync(default);

    var groups = matchTable.EnumerateAllMatchings()
        .ToDistinctGroups();

    var jaccard = new JaccardSimilarityService();
    var scores = jaccard.ComputeSimilarity(groups, store);


    Console.WriteLine($"Top {top} most similar files in: {rootPath}");

    var topScores = scores.GetAllScores()
                                  .OrderByDescending(x => x.Similarity)
                                  .Take(top)
                                  .ToList();

    if (!topScores.Any())
    {
        Console.WriteLine("No pairs with similarity found.");
        return 0;
    }

    foreach (var score in topScores)
    {
        var fileA = store.GetDocument(score.FileA)?.FullPath ?? "Unknown";
        var fileB = store.GetDocument(score.FileB)?.FullPath ?? "Unknown";
        Console.WriteLine($"{fileA} <-> {fileB} : {score.Similarity * 100:F2}%");
    }
}
catch (Exception e)
{
    Console.WriteLine($"Error: {e.Message}; {e.StackTrace}");
    return 1;
}

return 0;
