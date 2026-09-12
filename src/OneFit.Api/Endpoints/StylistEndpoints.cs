using OneFit.Application.Features.Stylist;

namespace OneFit.Api.Endpoints;

public static class StylistEndpoints
{
    public sealed record StylistMessageRequest(string? ShopperId, string? Message);

    public static void MapStylist(this WebApplication app)
    {
        app.MapPost("/stylist/message", async (
            StylistMessageRequest req,
            IStylistOrchestrator orchestrator,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.ShopperId))
                return Results.BadRequest(new { error = "shopper_id is required" });
            if (string.IsNullOrWhiteSpace(req.Message))
                return Results.BadRequest(new { error = "message is required" });

            var res = await orchestrator.HandleAsync(req.ShopperId.Trim(), req.Message.Trim(), ct);
            return Results.Ok(new
            {
                status = res.Status,
                reply = res.Reply,
                intent = new
                {
                    occasion = res.Intent.Occasion,
                    setting = res.Intent.Setting,
                    style = res.Intent.Style,
                    budget_egp = res.Intent.BudgetEgp,
                },
                outfits = res.Outfits,
                plan = res.Plan is null ? null : new
                {
                    occasion = res.Plan.Occasion,
                    budget = res.Plan.Budget,
                    item_slots = res.Plan.ItemSlots.Select(s => new
                    {
                        slot = s.Slot,
                        category = s.Category,
                        max_price_egp = s.MaxPriceEgp,
                        style_tags = s.StyleTags,
                    }),
                    reasoning = res.Plan.Reasoning,
                },
            });
        })
        .WithName("PostStylistMessage")
        .WithSummary("Stylist chat: free-text outfit need → intent → gated outfit assembly.");
    }
}
