namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public interface IUntypedDataPresenter
{
    Type ResponseType { get; }
    void Present(object response);
}
