using DiNet.HashSimilarityTK.CliToolkit.Abstraction;
using System.Collections.Concurrent;


namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public static class TypeExtensions
{
    public static Type GetPresenterDataType(this Type presenterType)
    {
        var presenterInterface = presenterType
            .GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDataPresenter<>));

        return presenterInterface?.GetGenericArguments().FirstOrDefault();
    }
}

public class PresenterStore : IPresenterStore
{
    private readonly ConcurrentDictionary<Type, Type> _presenters = new();

    internal void Register<TPresenter>()
    {
        var responseType = typeof(TPresenter).GetPresenterDataType();
        if (responseType == null) 
            throw new ArgumentException($"{typeof(TPresenter)} is not assignable to {typeof(IDataPresenter<>)}");

        Register(responseType, typeof(TPresenter));
    }

    public void Register<TResponse, TPresenter>()
        where TPresenter : IDataPresenter<TResponse>
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
