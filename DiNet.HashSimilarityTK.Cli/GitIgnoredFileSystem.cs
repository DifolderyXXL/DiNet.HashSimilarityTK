using DiNet.HashSimilarityTK.FileProcessing.Infrastructure;
using Ignore;

namespace DiNet.HashSimilarityTK.Cli;

public class GitIgnoredFileSystem : IFileSystem
{
    private readonly IgnoreRules _rules;

    public GitIgnoredFileSystem(string? ignoreFilePath)
    {
        _rules = new(File.Exists(ignoreFilePath)
            ? File.ReadAllLines(ignoreFilePath)
            : Array.Empty<string>());
    }

    public IEnumerable<string> EnumerateDirectories(string path)
    {
        return Directory.EnumerateDirectories(path)
            .Where(dir => !IsIgnored(path, dir, isDirectory: true));
    }

    public IEnumerable<string> EnumerateFiles(string path)
    {
        return Directory.EnumerateFiles(path)
            .Where(file => !IsIgnored(path, file, isDirectory: false));
    }

    public Stream OpenRead(string path)
    {
        string parentDir = Path.GetDirectoryName(path) ?? string.Empty;

        if (IsIgnored(parentDir, path, isDirectory: false))
        {
            throw new UnauthorizedAccessException($"Access denied. Path is ignored: {path}");
        }

        return File.OpenRead(path);
    }

    private bool IsIgnored(string basePath, string fullPath, bool isDirectory)
    {
        string relativePath = Path.GetRelativePath(basePath, fullPath);
        return _rules.IsIgnored(relativePath, isDirectory);
    }
}

public class IgnoreRules
{
    private readonly Ignore.Ignore _ignore = new();

    public IgnoreRules(IEnumerable<string> rules)
    {
        _ignore.Add(rules);
    }

    public bool IsIgnored(string relativePath, bool isDirectory)
    {
        string normalized = relativePath.Replace('\\', '/');

        if (isDirectory && !normalized.EndsWith("/"))
        {
            normalized += "/";
        }

        return _ignore.IsIgnored(normalized);
    }
}