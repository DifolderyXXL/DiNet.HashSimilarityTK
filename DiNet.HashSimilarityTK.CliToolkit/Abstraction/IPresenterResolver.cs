namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public interface IPresenterResolver
{
    IUntypedDataPresenter? TryResolve(Type responseType);
}
