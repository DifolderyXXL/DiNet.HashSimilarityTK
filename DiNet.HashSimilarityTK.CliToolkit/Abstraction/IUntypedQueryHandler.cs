namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction;

public interface IUntypedQueryHandler
{
    Type QueryType { get; }
    Task<object> HandleAsync(object command, CancellationToken ct);
}
