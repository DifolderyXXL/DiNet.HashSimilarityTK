using DiNet.HashSimilarityTK.FileProcessing.Infrastructure.FileSystem;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.Indexing;

public class FileSystemIndexer(IFileSystem fileSystem)
{
    private long _nextId = 1;

    public DocumentStore BuildIndex(string rootPath)
    {
        var store = new DocumentStore();
        IndexRecursive(rootPath, store);
        return store;
    }

    private void IndexRecursive(string path, DocumentStore store)
    {
        foreach (var dir in fileSystem.EnumerateDirectories(path))
            IndexRecursive(dir, store);

        foreach (var file in fileSystem.EnumerateFiles(path))
        {
            var doc = new Document(fileSystem, file, _nextId++);
            store.AddDocument(doc);
        }
    }
}
