using OneFit.Application.Features.Products;

namespace OneFit.Application.Features.Chatbot;

public sealed class ChatbotOrchestrator : IChatbotOrchestrator
{
    private const string OffTopicPrefix = "Sorry, your request is not clear";

    private readonly IChatbotClient _client;
    private readonly IProductQueryService _products;

    public ChatbotOrchestrator(IChatbotClient client, IProductQueryService products)
    {
        _client = client;
        _products = products;
    }

    public async Task<ChatbotChatResult> HandleAsync(string message, bool newChat, CancellationToken ct = default)
    {
        var raw = await _client.GetRecommendationsAsync(message, newChat, ct);
        var cleaned = raw
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .ToList();

        if (cleaned.Count == 1 && IsOffTopic(cleaned[0]))
            return new ChatbotChatResult("off_topic", cleaned[0], [], [], []);

        var ids = cleaned.Distinct().ToList();
        var products = new List<ProductDetailDto>();
        var missing = new List<string>();

        foreach (var id in ids)
        {
            var product = await _products.GetByIdAsync(id, ct);
            if (product is null)
                missing.Add(id);
            else
                products.Add(product);
        }

        return new ChatbotChatResult("ready", "Here are picks matching your request.", ids, products, missing);
    }

    private static bool IsOffTopic(string text) =>
        text.StartsWith(OffTopicPrefix, StringComparison.OrdinalIgnoreCase);
}
