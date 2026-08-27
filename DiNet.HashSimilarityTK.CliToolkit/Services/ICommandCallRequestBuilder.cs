using DiNet.HashSimilarityTK.CliToolkit.Core.Models;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public interface ICommandCallRequestBuilder
{
    public CommandCallRequest Build(ReadOnlySpan<string> args);
}
