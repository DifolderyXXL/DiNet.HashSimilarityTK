using DiNet.HashSimilarityTK.CliToolkit.Core.Models;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure;

public class DefaultCommandCallRequestBuilder : ICommandCallRequestBuilder
{   
    public CommandCallRequest Build(ReadOnlySpan<string> args)
    {
        string? firstUnflagged = null;
        var parameters = new List<ConsoleParameter>();

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (arg.StartsWith("-"))
            {
                var name = arg.TrimStart('-');
                string? value = null;

                var equalsIndex = name.IndexOf('=');
                if (equalsIndex != -1)
                {
                    value = name[(equalsIndex + 1)..];
                    name = name[..equalsIndex];
                }
                else if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                {
                    value = args[++i];
                }

                parameters.Add(new ConsoleParameter(name, value));
            }
            else if (firstUnflagged is null)
            {
                firstUnflagged = arg;
            }
        }

        return new CommandCallRequest(firstUnflagged, parameters.ToArray());
    }
}
