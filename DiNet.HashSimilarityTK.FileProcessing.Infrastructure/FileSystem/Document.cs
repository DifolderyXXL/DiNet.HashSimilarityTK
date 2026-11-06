using DiNet.HashSimilarityTK.Core;
using DiNet.HashSimilarityTK.FileProcessing.Core;
using System.Text;

namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.FileSystem;

public class Document(IFileSystem fileSystem, string fullPath, long id) : IDocument
{
    public long Id => id;
    public string FullPath => fullPath;

    public TextReader OpenTextReader()
    {
        var stream = fileSystem.OpenRead(fullPath);
        return new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
    }

    public HashSet<Hash> StringHashes { get; } = new HashSet<Hash>();
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
