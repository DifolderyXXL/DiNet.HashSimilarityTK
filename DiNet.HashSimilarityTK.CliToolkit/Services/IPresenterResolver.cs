using DiNet.HashSimilarityTK.CliToolkit.Abstraction;

namespace DiNet.HashSimilarityTK.CliToolkit.Services;

public interface IPresenterResolver
{
    IUntypedDataPresenter? TryResolve(Type responseType);
}
