using DiNet.HashSimilarityTK.CliToolkit.Core.Models;

namespace DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

public interface ICommandHandlerFactory
{
    T Create<T>(CommandCallRequest request);
    object Create(Type type, CommandCallRequest request);
}
