using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.Infrastructure;
using DiNet.HashSimilarityTK.MetricsEngine.Application;
using DiNet.HashSimilarityTK.MetricsEngine.Core;
using DiNet.HashSimilarityTK.MetricsEngine.Infrastructure;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

var matchTable = new MatchSet<DocumentFileLine>(
    shingleSize: 4,
    numHashes: 128,
    chunkStep: 16,
    seed: 123456789
);


var fileSystem = new SimpleFileSystem();
var indexer = new DocumentIndexer(fileSystem);

var store = indexer.BuildIndex(@"C:\C#\Leasure\DiNet.HashSimilarityTK\DiNet.HashSimilarityTK.Console\TestRoot\");


var service = new DocumentProcessingService(store, matchTable, (document, line) =>
{
    return new(document.Id, line);
});

await service.ProcessAllDocumentsAsync(default);



var groups = matchTable.EnumerateAllMatchings()
    .ToDistinctGroups();

var jaccard = new JaccardSimilarityService();
var scores = jaccard.ComputeSimilarity(groups, new DocumentLineCountProvider(store));

foreach (var score in scores.GetAllScores().OrderByDescending(x => x.Similarity))
{
    Console.WriteLine($"{store.GetDocument(score.FileA).FullPath} == {store.GetDocument(score.FileB).FullPath} on {score.Similarity * 100.0}%");
}
