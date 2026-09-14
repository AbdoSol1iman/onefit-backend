using OneFit.Application.Features.Chatbot;

namespace OneFit.Api.Endpoints;

public static class ChatbotEndpoints
{
    public sealed record ChatbotMessageRequest(string? ShopperId, string? Message, bool? NewChat);

    public static void MapChatbot(this WebApplication app)
    {
        app.MapPost("/chatbot/message", async (
            ChatbotMessageRequest req,
            IChatbotOrchestrator orchestrator,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.ShopperId))
                return Results.BadRequest(new { error = "shopper_id is required" });
            if (string.IsNullOrWhiteSpace(req.Message))
                return Results.BadRequest(new { error = "message is required" });

            try
            {
                var res = await orchestrator.HandleAsync(
                    req.Message.Trim(),
                    req.NewChat ?? true,
                    ct);

                return Results.Ok(new
                {
                    status = res.Status,
                    reply = res.Reply,
                    product_ids = res.ProductIds,
                    products = res.Products,
                    missing_ids = res.MissingIds,
                });
            }
            catch (HttpRequestException)
            {
                return Results.Json(
                    new { error = "chatbot service unavailable" },
                    statusCode: StatusCodes.Status502BadGateway);
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                return Results.Json(
                    new { error = "chatbot service timed out" },
                    statusCode: StatusCodes.Status504GatewayTimeout);
            }
        })
        .WithName("PostChatbotMessage")
        .WithSummary("Proxy to mohamedtamer00/chatbot docker (POST /chatbot/?UserQuery=&NewChat=) and enrich returned IDs into full product details.");
    }
}
