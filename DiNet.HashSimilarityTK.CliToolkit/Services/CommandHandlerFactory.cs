using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using DiNet.HashSimilarityTK.CliToolkit.Core.Models;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public class CommandHandlerFactory(ICommandCallerStore store, IParameterBinder binder) : ICommandHandlerFactory
{
    public T Create<T>(CommandCallRequest request)
    {
        return (T)Create(typeof(T), request);
    }

    public object Create(Type type, CommandCallRequest request)
    {
        var caller = store.Get(type)
           ?? throw new InvalidOperationException($"Command handler for type '{type.FullName}' is not registered or cannot be resolved.");

        var arguments = binder.Bind(caller, request);

        return caller.Constructor.Invoke(arguments);
    }
}
