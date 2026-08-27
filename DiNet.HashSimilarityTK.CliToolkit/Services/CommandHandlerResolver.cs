using DiNet.HashSimilarityTK.CliToolkit.Core;
using DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;
using DiNet.HashSimilarityTK.CliToolkit.Core.Models;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public class CommandHandlerResolver(ICommandCallerStore store, IParameterBinder binder)
{
    public T Resolve<T>(CommandCallRequest request)
    {
        var caller = store.Get<T>() ?? throw new Exception("Handler does not exist");
        var arguments = binder.Bind(caller, request);
        return (T)caller.Constructor.Invoke(arguments);
    }

    public object Resolve(Type type, CommandCallRequest request)
    {
        var caller = store.Get(type) ?? throw new Exception("Handler does not exist");
        var arguments = binder.Bind(caller, request);
        return caller.Constructor.Invoke(arguments);
    }
}

public class ForcedCommandCallerStore(ICommandNameFormatter formatter) : ICommandCallerStore
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
