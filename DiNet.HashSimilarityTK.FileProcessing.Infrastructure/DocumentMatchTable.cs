using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using System.Reflection.PortableExecutable;
using System.Text;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure;

public class DocumentMatchTable<TDocumentKey>(
    ITextMatchTable<DocumentFileLine> matchTable,
    Func<IDocument, long, DocumentFileLine> indexer) : IDocumentMatchTable<TDocumentKey>
{
    public async Task Add(IDocument document, CancellationToken ct = default)
    {
        long lineNumber = 0;
        using var reader = document.OpenTextReader();
        string? line;
        while ((line = await reader.ReadLineAsync(ct)) != null)
        {
            var key = indexer(document, lineNumber);
            matchTable.Add(line, key);
            lineNumber++;
        }
    }

    public IEnumerable<IEnumerable<DocumentFileLine>> GetAllMatches()
        => matchTable.EnumerateAllMatchings();
}


public interface IFileSystem
{
    IEnumerable<string> EnumerateDirectories(string path);
    IEnumerable<string> EnumerateFiles(string path);
    Stream OpenRead(string path);
}

public class SimpleFileSystem : IFileSystem
{
    public IEnumerable<string> EnumerateDirectories(string path)
    {
        return Directory.EnumerateDirectories(path);
    }

    public IEnumerable<string> EnumerateFiles(string path)
    {
        return Directory.EnumerateFiles(path);
    }

    public Stream OpenRead(string path)
    {
        return File.OpenRead(path);
    }
}

public class Document(IFileSystem fileSystem, string fullPath, long id) : IDocument
{
    public long Id => id;
    public string FullPath => fullPath;

    public TextReader OpenTextReader()
    {
        var stream = fileSystem.OpenRead(fullPath);
        return new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
    }


    private long? _lineCount;

    public long LineCount
    {
        get
        {
            if (_lineCount == null)
            {
                using var reader = OpenTextReader();
                long count = 0;
                while (reader.ReadLine() != null) count++;
                _lineCount = count;
            }
            return _lineCount.Value;
        }
    }
    internal void SetLineCount(long count) => _lineCount = count;
}

public class TreeRoot : ITreeRoute
{
    public required long Id { get; init; }

    public required string FullPath { get; init; }
}

public class DocumentDirectory( 
    string fullPath,
    long id,
    List<IDocumentDirectory> directories,
    List<IDocument> documents
    ) : IDocumentDirectory
{
    public long Id => id;

    public string FullPath => fullPath;

    public long DocumentCount => documents.Count;

    public long DirectoryCount => directories.Count;


    public IEnumerable<IDocumentDirectory> GetDirectories()
    {
        return directories;
    }

    public IEnumerable<IDocument> GetDocuments()
    {
        return documents;
    }
}

public class DocumentIndexer(IFileSystem fileSystem)
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

public class DocumentStore : IDocumentStore
{
    private readonly Dictionary<long, IDocument> _documents = new();

    public long DocumentCount => _documents.Count;

    public void AddDocument(IDocument doc) => _documents[doc.Id] = doc;

    public IDocument? GetDocument(long id) => _documents.GetValueOrDefault(id);

    public IEnumerable<IDocument> GetAllDocuments() => _documents.Values;
}



public class DocumentProcessingService(
    IDocumentStore store,
    ITextMatchTable<DocumentFileLine> matchTable,
    Func<IDocument, long, DocumentFileLine> indexer)
{

    public async Task ProcessAllDocumentsAsync(CancellationToken ct)
    {
        foreach (var doc in store.GetAllDocuments())
        {
            long lineNumber = 0;
            using var reader = doc.OpenTextReader();
            string? line;
            while ((line = await reader.ReadLineAsync(ct)) != null)
            {
                var key = indexer(doc, lineNumber);
                matchTable.Add(line, key);
                lineNumber++;
            }

            ((Document)doc)?.SetLineCount(lineNumber);
        }
    }
}