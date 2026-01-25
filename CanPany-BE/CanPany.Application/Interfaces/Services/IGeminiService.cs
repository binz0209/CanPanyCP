namespace CanPany.Application.Interfaces.Services;

public interface IGeminiService
{
    Task<List<double>> GenerateEmbeddingAsync(string text);
    Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage);
}
