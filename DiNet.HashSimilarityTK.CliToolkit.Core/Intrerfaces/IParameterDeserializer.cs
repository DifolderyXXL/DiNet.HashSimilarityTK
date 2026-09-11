namespace DiNet.HashSimilarityTK.CliToolkit.Core.Intrerfaces;

public interface IParameterDeserializer
{
    object? Deserialize(ParameterSource source, Type targetType);
}
public record ParameterSource(string? Name, string? Value);
