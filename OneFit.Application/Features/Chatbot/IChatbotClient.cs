namespace OneFit.Application.Features.Chatbot;

public sealed class ChatbotOptions
{
    public const string SectionName = "Chatbot";
    public string BaseUrl { get; set; } = "http://127.0.0.1:8000";
    public int TimeoutSeconds { get; set; } = 30;
}

public interface IChatbotClient
{
    Task<List<string>> GetRecommendationsAsync(string userQuery, bool newChat, CancellationToken ct = default);
}
