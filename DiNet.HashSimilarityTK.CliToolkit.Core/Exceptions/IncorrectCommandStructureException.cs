namespace DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;

[Serializable]
public class IncorrectCommandStructureException : Exception
{
    public IncorrectCommandStructureException()
    {
        
    }
    public IncorrectCommandStructureException(string message) : base(message)
    {

    }
}
