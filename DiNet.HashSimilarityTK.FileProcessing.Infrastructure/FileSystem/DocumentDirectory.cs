using DiNet.HashSimilarityTK.FileProcessing.Core;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.FileSystem;

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
