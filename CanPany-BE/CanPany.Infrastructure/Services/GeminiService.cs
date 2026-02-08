using System.Text;
using System.Text.Json;
using CanPany.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CanPany.Infrastructure.Services;

public class GeminiService : IGeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GeminiService> _logger;
    private readonly string _apiKey;
    private readonly string _embeddingModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/embedding-001:embedContent";
    private readonly string _chatModelUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent";

    public GeminiService(
        HttpClient httpClient,
        IConfiguration configuration,
        ILogger<GeminiService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _apiKey = _configuration["GoogleGemini:ApiKey"] ?? string.Empty;
    }

    public async Task<List<double>> GenerateEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            _logger.LogWarning("Gemini API Key is missing or empty. Returning mock embedding.");
            return GenerateMockEmbedding();
        }

        try
        {
            var requestBody = new
            {
                model = "models/embedding-001",
                content = new { parts = new[] { new { text = text } } }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_embeddingModelUrl}?key={_apiKey}", content);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            
            var values = doc.RootElement
                .GetProperty("embedding")
                .GetProperty("values");

            var result = new List<double>();
            foreach (var value in values.EnumerateArray())
            {
                result.Add(value.GetDouble());
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding with Gemini API");
            return GenerateMockEmbedding();
        }
    }

    public async Task<string> GenerateChatResponseAsync(string systemPrompt, string userMessage)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            return "Gemini API Key is missing. Please configure it in appsettings.json.";
        }

        try
        {
            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        parts = new[] { new { text = systemPrompt + "\nUser: " + userMessage } }
                    }
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_chatModelUrl}?key={_apiKey}", content);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseString);
            
            var text = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text ?? "No response from AI.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating chat response with Gemini API");
            return "Sorry, I encountered an error while processing your request.";
        }
    }

    private List<double> GenerateMockEmbedding()
    {
        // Generate a random 768-dimensional vector (standard for many models, though embedding-001 is 768)
        var random = new Random();
        var embedding = new List<double>();
        for (int i = 0; i < 768; i++)
        {
            embedding.Add(random.NextDouble());
        }
        return embedding;
    }
}
