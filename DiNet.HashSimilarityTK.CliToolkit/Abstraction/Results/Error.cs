namespace DiNet.HashSimilarityTK.CliToolkit.Abstraction.Results;

public record Error(string Message, string? Code = null, Exception? InnerException = null)
{
    public static implicit operator Error(string message) => new(message);
    public static implicit operator Error(Exception ex) => new(ex.Message, InnerException: ex);
}
