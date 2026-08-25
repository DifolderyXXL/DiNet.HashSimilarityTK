namespace DiNet.HashSimilarityTK.CliToolkit.Core.Exceptions;

[Serializable]
public class IncorrectCommandDefenitionException : Exception
{
    public IncorrectCommandDefenitionException()
    {
        
    }
    public IncorrectCommandDefenitionException(string message) : base(message)
    {
        
    }
}
