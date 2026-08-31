using DiNet.HashSimilarityTK.CliToolkit.Core.Models;

namespace DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

public interface ICommandCallRequestBuilder
{
    public CommandCallRequest Build(ReadOnlySpan<string> args);
}
