using OneFit.Application.Features.Products;

namespace OneFit.Application.Features.Chatbot;

public sealed record ChatbotChatResult(
    string Status,
    string Reply,
    List<string> ProductIds,
    List<ProductDetailDto> Products,
    List<string> MissingIds
);

public interface IChatbotOrchestrator
{
    Task<ChatbotChatResult> HandleAsync(string message, bool newChat, CancellationToken ct = default);
}
