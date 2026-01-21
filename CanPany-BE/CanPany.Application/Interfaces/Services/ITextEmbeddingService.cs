namespace CanPany.Application.Interfaces.Services;

/// <summary>
/// Generates vector embeddings for text for semantic search.
/// </summary>
public interface ITextEmbeddingService
{
    /// <summary>
    /// Create an embedding vector for a text input.
    /// </summary>
    List<double> Embed(string text);
}



