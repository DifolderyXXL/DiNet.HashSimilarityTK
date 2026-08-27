using DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;

namespace DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;

[Serializable]
public class InvalidParameterProvidedException : IncorrectCommandStructureException
{
    public InvalidParameterProvidedException()
    {
        
    }
    public InvalidParameterProvidedException(string message) : base(message)
    {
        
    }
}
