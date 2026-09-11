namespace DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

public interface ICommandCallerStore
{
    public CommandCaller? Get<T>();
    public CommandCaller? Get(Type type);
}
