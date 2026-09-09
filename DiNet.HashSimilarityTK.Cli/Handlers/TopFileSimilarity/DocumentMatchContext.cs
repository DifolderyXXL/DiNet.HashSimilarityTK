using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using DiNet.HashSimilarityTK.Infrastructure;

namespace DiNet.HashSimilarityTK.Cli.Handlers.TopFileSimilarity;

public record DocumentMatchContext(
    DistinctMatch<DocumentFileLine> Groups,
    IDocumentStore Store
);

public interface IDocumentMatchService
{
    Task<DocumentMatchContext> ExtractMatchGroupsAsync(
        string? rootPath,
        int shingleSize,
        int numHashes,
        int chunkStep,
        uint seed,
        string? ignore,
        CancellationToken ct = default);
}

public class DocumentMatchService : IDocumentMatchService
{
    public async Task<DocumentMatchContext> ExtractMatchGroupsAsync(
        string? rootPath,
        int shingleSize,
        int numHashes,
        int chunkStep,
        uint seed,
        string? ignore,
        CancellationToken ct = default)
    {
        var matchTable = new MatchSet<DocumentFileLine>(
            shingleSize: shingleSize,
            numHashes: numHashes,
            chunkStep: chunkStep,
            seed: seed
        );

        var root = rootPath ?? Environment.CurrentDirectory;
        var fileSystem = new GitIgnoredFileSystem(ignore);
        var indexer = new DocumentIndexer(fileSystem);

        var store = indexer.BuildIndex(root);

        var processingService = new DocumentProcessingService(
            store,
            new XxHasher(seed),
            matchTable,
            (document, hash, line) => new DocumentFileLine(document.Id, line, hash)
        );

        await processingService.ProcessAllDocumentsAsync(ct);

        var groups = matchTable.EnumerateAllMatchings().ToDistinctGroups();

        return new DocumentMatchContext(groups, store);
    }
}