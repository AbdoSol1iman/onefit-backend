using OneFit.Application.Features.Products;

namespace OneFit.Application.Features.Stylist;

public sealed class StylistOrchestrator(IStylistSessionStore sessions, IProductQueryService products) : IStylistOrchestrator
{
    public async Task<StylistResult> HandleAsync(string shopperId, string message, CancellationToken ct = default)
    {
        var session = sessions.GetOrCreate(shopperId);
        var trimmed = message.Trim();

        if (StylistIntentExtractor.IsSkip(trimmed) && session.AwaitingBudget)
        {
            var skipped = session.Intent with { BudgetSkipped = true };
            sessions.Save(shopperId, new StylistSession(skipped, AwaitingBudget: false));
            return await AssembleAsync(skipped, ct);
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
        return await AssembleAsync(merged, ct);
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
