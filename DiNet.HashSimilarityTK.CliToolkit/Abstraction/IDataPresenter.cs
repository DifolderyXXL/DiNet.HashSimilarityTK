namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public interface IDataPresenter<in TResponse>
{
    void Present(TResponse response);
}
