using OneFit.Application.Features.Stylist;
using System.Security.Claims;

namespace OneFit.Api.Endpoints;

public static class StylistEndpoints
{
    public sealed record StylistMessageRequest(string? Message);

    public static void MapStylist(this WebApplication app)
    {
        app.MapPost("/stylist/message", async (
            StylistMessageRequest req,
            ClaimsPrincipal user,
            IStylistOrchestrator orchestrator,
            CancellationToken ct) =>
        {
            // Extract ShopperId from JWT — not from request body
            var shopperId = user.FindFirstValue("UserId");
            if (string.IsNullOrWhiteSpace(shopperId))
                return Results.Unauthorized();

            if (string.IsNullOrWhiteSpace(req.Message))
                return Results.BadRequest(new { error = "message is required" });

            var res = await orchestrator.HandleAsync(shopperId.Trim(), req.Message.Trim(), ct);
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
        .RequireAuthorization()
        .RequireRateLimiting("stylist")
        .WithName("PostStylistMessage")
        .WithSummary("Stylist chat: free-text outfit need → intent → gated outfit assembly.");
    }
}
