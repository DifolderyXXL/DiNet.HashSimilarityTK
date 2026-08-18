using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.Infrastructure;
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
    .ToDistinctGroups()
    .Result;

var intersections = new Dictionary<(long, long), HashSet<int>>();

var groupId = 0;
foreach (var group in groups)
{
    var files = group.Select(e => e.DocumentId).Distinct().ToArray();

    for(int i = 0; i < files.Length; ++i)
    {
        for (int j = i+1; j < files.Length; j++)
        {
            var key = (Math.Min(files[i], files[j]), Math.Max(files[i], files[j]));
            if(!intersections.TryGetValue(key, out var hashSet))
            {
                hashSet = new();
                intersections.Add(key, hashSet);
            }

            hashSet.Add(groupId);
        }
    }

    ++groupId;
}

var scores = new Dictionary<(long, long), double>();
foreach (var intersection in intersections)
{
    var intersectionSize = intersection.Value.Count;

    var unionSize = 
        store.GetDocument(intersection.Key.Item1)!.LineCount 
        + store.GetDocument(intersection.Key.Item2)!.LineCount 
        - intersectionSize;

    scores.Add(intersection.Key, 1.0 * intersectionSize / unionSize);
}

foreach (var score in scores.OrderBy(x=>x.Value))
{
    Console.WriteLine($"{store.GetDocument(score.Key.Item1).FullPath} == {store.GetDocument(score.Key.Item2).FullPath} on {score.Value * 100.0}%");
}
