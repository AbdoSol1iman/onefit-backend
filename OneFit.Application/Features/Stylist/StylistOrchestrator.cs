using OneFit.Application.Features.Products;
using OneFit.Application.Features.Stylist.Gemini;

namespace OneFit.Application.Features.Stylist;

public sealed class StylistOrchestrator(
    IStylistSessionStore sessions,
    IProductQueryService products,
    IGeminiOutfitPlanner gemini) : IStylistOrchestrator
{
    public async Task<StylistResult> HandleAsync(string shopperId, string message, bool newChat = false, CancellationToken ct = default)
    {
        if (newChat)
            sessions.Reset(shopperId);

        var session = sessions.GetOrCreate(shopperId);
        var trimmed = message.Trim();

        if (StylistIntentExtractor.IsSkip(trimmed) && session.AwaitingBudget)
        {
            var skipped = session.Intent with { BudgetSkipped = true };
            sessions.Save(shopperId, new StylistSession(skipped, AwaitingBudget: false));
            return await ResolveAsync(skipped, ct);
        }

        var fresh = StylistIntentExtractor.Extract(trimmed);
        var merged = StylistIntentExtractor.Merge(session.Intent, fresh);

        if (!StylistIntentExtractor.HasAnyField(merged) &&
            !StylistIntentExtractor.LooksFashionRelated(trimmed))
        {
            return new StylistResult(
                "off_topic",
                "I can help you build an outfit — tell me the occasion, budget, or style you're after",
                merged,
                [],
                CatalogCalled: false);
        }

        if (merged.BudgetEgp is null && !merged.BudgetSkipped)
        {
            sessions.Save(shopperId, new StylistSession(merged, AwaitingBudget: true));
            return new StylistResult(
                "need_budget",
                "What's your budget for this look?",
                merged,
                [],
                CatalogCalled: false);
        }

        sessions.Save(shopperId, new StylistSession(merged, AwaitingBudget: false));
        return await ResolveAsync(merged, ct);
    }

    private async Task<StylistResult> ResolveAsync(StylistIntent intent, CancellationToken ct) =>
        await gemini.PlanAsync(intent, ct) switch
        {
            PlanSkipped => await AssembleAsync(intent, ct),
            PlanSuccess s => await AssembleFromPlanAsync(intent, s.Plan, ct),
            PlanRateLimited => new StylistResult(
                "stylist_busy",
                "Our stylist is a bit busy right now — please try again in a moment",
                intent, [], CatalogCalled: false),
            PlanFailed => new StylistResult(
                "llm_fallback",
                "I'm having trouble putting that look together — try rephrasing your request",
                intent, [], CatalogCalled: false),
            _ => await AssembleAsync(intent, ct),
        };

    private async Task<StylistResult> AssembleFromPlanAsync(StylistIntent intent, GeminiOutfitPlan plan, CancellationToken ct)
    {
        var outfits = new List<ProductSummaryDto>();
        foreach (var slot in plan.ItemSlots.Take(3))
        {
            var page = await products.ListAsync(new ProductListQuery(
                Category: slot.Category,
                MaxPriceEgp: slot.MaxPriceEgp ?? intent.BudgetEgp ?? plan.Budget,
                Search: null,
                InStockOnly: true,
                Sort: "price_asc",
                Page: 1,
                PageSize: 20,
                StyleTags: slot.StyleTags ?? StylistIntentExtractor.ToStyleTags(intent.Style),
                StyleMatch: "rank",
                Limit: 1), ct);
            var first = page.Items.FirstOrDefault();
            if (first is not null)
                outfits.Add(first);
        }

        return new StylistResult("ready", "Assembling your outfits", intent, outfits, CatalogCalled: true, Plan: plan);
    }

    private async Task<StylistResult> AssembleAsync(StylistIntent intent, CancellationToken ct)
    {
        var query = new ProductListQuery(
            Category: null,
            MaxPriceEgp: intent.BudgetEgp,
            Search: null,
            InStockOnly: true,
            Sort: "price_asc",
            Page: 1,
            PageSize: 20,
            StyleTags: StylistIntentExtractor.ToStyleTags(intent.Style),
            StyleMatch: "rank",
            Limit: 3);

        var page = await products.ListAsync(query, ct);
        return new StylistResult(
            "ready",
            "Assembling your outfits",
            intent,
            page.Items,
            CatalogCalled: true);
    }
}
