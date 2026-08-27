using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using System.Collections.Concurrent;


namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public class PresenterStore : IPresenterStore
{
    private readonly ConcurrentDictionary<Type, Type> _presenters = new();

    public void Register<TResponse, TPresenter>()
        where TPresenter : IConsolePresenter<TResponse>
    {
        Register(typeof(TResponse), typeof(TPresenter));
    }

    public void Register(Type responseType, Type presenterType)
    {
        ArgumentNullException.ThrowIfNull(responseType);
        ArgumentNullException.ThrowIfNull(presenterType);

        _presenters[responseType] = presenterType;
    }

    public Type? GetPresenterTypeFor(Type responseType)
    {
        ArgumentNullException.ThrowIfNull(responseType);

        if (_presenters.TryGetValue(responseType, out var directPresenter))
        {
            return directPresenter;
        }

        foreach (var (registeredType, presenter) in _presenters)
        {
            if (registeredType.IsAssignableFrom(responseType))
            {
                return presenter;
            }
        }

        return null;
    }
}
