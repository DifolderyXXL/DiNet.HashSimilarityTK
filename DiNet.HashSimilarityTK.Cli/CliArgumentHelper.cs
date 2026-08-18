namespace DiNet.HashSimilarityTK.Cli;

public class CliArgumentHelper
{
    private readonly string[] _args;

    private CliArgumentHelper(string[] args)
    {
        _args = args;
    }

    public static CliArgumentHelper Create(string[] args) => new(args);


    public string? GetFlagArgument(string flagName)
    {
        for (int i = 0; i < _args.Length - 1; i++)
        {
            if (_args[i].Equals(flagName, StringComparison.OrdinalIgnoreCase))
                return _args[i + 1];
        }

        return null;
    }

    public string GetString(string flagName, string defaultValue = "")
    {
        return GetFlagArgument(flagName) ?? defaultValue;
    }

    public int GetInt(string flagName, int defaultValue)
    {
        var value = GetFlagArgument(flagName);
        if (value != null && int.TryParse(value, out var intValue))
            return intValue;
        return defaultValue;
    }

    public string GetPositional(int index, string defaultValue = "")
    {
        var nonFlagArgs = _args.Where(a => !a.StartsWith("--")).ToArray();
        return index < nonFlagArgs.Length ? nonFlagArgs[index] : defaultValue;
    }
}
