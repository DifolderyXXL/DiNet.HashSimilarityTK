using DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;
using DiNet.HashSimilarityTK.CliToolkit.Core.Models;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure;

/// <summary>
/// Expects strict argument layout:
/// <c>firstValue --flag Value --flag2 Value --boolFlag</c>
/// </summary>
/// <remarks>
/// - The first token without '-' prefix is captured as <see cref="CommandCallRequest.FirstUnflaggedArgument"/>.
/// - Every following token must be a flag ('-name' or '--name'); a value is consumed
///   from the next token unless it looks like another flag (boolean flag).
/// - Unexpected positional tokens and duplicate flags are rejected with an exception
///   instead of being silently dropped.
/// </remarks>
public class StrictCommandCallRequestBuilder : ICommandCallRequestBuilder
{
    public CommandCallRequest Build(ReadOnlySpan<string> args)
    {
        string? firstUnflagged = null;
        var parameters = new List<ConsoleParameter>();
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

            if (!arg.StartsWith('-'))
            {
                if (firstUnflagged is null)
                {
                    firstUnflagged = arg;
                    continue;
                }

                throw new IncorrectCommandStructureException(
                    $"Unexpected positional argument '{arg}' at position {i}.");
            }

            var name = arg.TrimStart('-');

            if (name.Length == 0)
                throw new InvalidParameterProvidedException($"Empty flag name at position {i}.");

            if (!seenNames.Add(name))
                throw new InvalidParameterProvidedException($"Duplicate flag '{name}'.");

            string? value = null;

            var equalsIndex = name.IndexOf('=');
            if (equalsIndex != -1)
            {
                value = name[(equalsIndex + 1)..];
                name = name[..equalsIndex];
            }
            else if (i + 1 < args.Length && !IsFlag(args[i + 1]))
            {
                value = args[++i];
            }

            parameters.Add(new ConsoleParameter(name, value));
        }

        return new CommandCallRequest(firstUnflagged, parameters.ToArray());
    }

    private static bool IsFlag(string token)
        => token.StartsWith('-') && !IsNegativeNumber(token);

    private static bool IsNegativeNumber(string token)
        => token.Length > 1 && char.IsDigit(token[1]);
}
