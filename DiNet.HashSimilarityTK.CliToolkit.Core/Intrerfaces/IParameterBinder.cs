using DiNet.HashSimilarityTK.CliToolkit.Core.Models;

namespace DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

public interface IParameterBinder
{
    object?[] Bind(CommandCaller caller, CommandCallRequest request);
}
