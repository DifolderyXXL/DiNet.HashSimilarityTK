namespace DiNet.HashSimilarityTK.FileProcessing.Core;

public interface IDocumentLineFilter
{
    /// <summary>
    /// Evaluates a document line to determine whether it should be included in indexing.
    /// </summary>
    /// <param name="line"></param>
    /// <returns>
    /// <c>true</c>, if line is valid to process; 
    /// <c>false</c>, if is NOT valid, should be skipped
    /// </returns>
    public bool FilterLine(string line);
}
