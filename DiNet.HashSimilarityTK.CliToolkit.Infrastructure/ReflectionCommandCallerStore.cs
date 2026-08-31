using DiNet.HashSimilarityTK.CliToolkit.Core;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

namespace DiNet.HashSimilarityTK.CliToolkit.Infrastructure;

public class ReflectionCommandCallerStore(ICommandNameFormatter formatter) : ICommandCallerStore
{
    public CommandCaller? Get<T>()
    {
        return CommandCaller.CreateFor(typeof(T), formatter);
    }

    public CommandCaller? Get(Type type)
    {
        return CommandCaller.CreateFor(type, formatter);
    }
}
