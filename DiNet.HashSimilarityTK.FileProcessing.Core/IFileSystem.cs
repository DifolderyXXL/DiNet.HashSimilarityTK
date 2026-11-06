namespace DiNet.HashSimilarityTK.FileProcessing.Infrastructure.FileSystem;

public interface IFileSystem
{
    IEnumerable<string> EnumerateDirectories(string path);
    IEnumerable<string> EnumerateFiles(string path);
    Stream OpenRead(string path);
}
