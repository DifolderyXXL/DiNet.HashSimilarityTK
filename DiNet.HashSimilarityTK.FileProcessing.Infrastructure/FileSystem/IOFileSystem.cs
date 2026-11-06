namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.FileSystem;

public class IOFileSystem : IFileSystem
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
