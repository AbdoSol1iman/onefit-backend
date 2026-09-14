using System.Net.Http.Json;

namespace OneFit.Application.Features.Chatbot;

public sealed class ChatbotClient : IChatbotClient
{
    private readonly HttpClient _http;

    public ChatbotClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<string>> GetRecommendationsAsync(string userQuery, bool newChat, CancellationToken ct = default)
    {
        var url = $"chatbot/?UserQuery={Uri.EscapeDataString(userQuery)}&NewChat={newChat.ToString().ToLowerInvariant()}";
        using var content = new StringContent(string.Empty);
        content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
        using var response = await _http.PostAsync(url, content, ct);
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<List<string>>(cancellationToken: ct);
        return items ?? [];
    }
}
