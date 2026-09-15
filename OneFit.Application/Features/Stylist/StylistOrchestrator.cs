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
            sessions.Save(shopperId, session with { Intent = skipped, AwaitingBudget = false });
            return await ResolveAsync(shopperId, skipped, ct);
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
            sessions.Save(shopperId, session with { Intent = merged, AwaitingBudget = true });
            return new StylistResult(
                "need_budget",
                "What's your budget for this look?",
                merged,
                [],
                CatalogCalled: false);
        }

        sessions.Save(shopperId, session with { Intent = merged, AwaitingBudget = false });
        return await ResolveAsync(shopperId, merged, ct);
    }

    private async Task<StylistResult> ResolveAsync(string shopperId, StylistIntent intent, CancellationToken ct)
    {
        var shown = new HashSet<string>(
            sessions.GetOrCreate(shopperId).ShownProductIds ?? [],
            StringComparer.OrdinalIgnoreCase);

        var result = await gemini.PlanAsync(intent, ct) switch
        {
            PlanSkipped => await AssembleAsync(intent, shown, ct),
            PlanSuccess s => await AssembleFromPlanAsync(intent, s.Plan, shown, ct),
            PlanRateLimited => new StylistResult(
                "stylist_busy",
                "Our stylist is a bit busy right now — please try again in a moment",
                intent, [], CatalogCalled: false),
            PlanFailed => new StylistResult(
                "llm_fallback",
                "I'm having trouble putting that look together — try rephrasing your request",
                intent, [], CatalogCalled: false),
            _ => await AssembleAsync(intent, shown, ct),
        };

        if (result.Outfits.Count > 0)
        {
            var unique = result.Outfits
                .GroupBy(o => o.ProductId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();
            foreach (var outfit in unique)
                shown.Add(outfit.ProductId);
            var current = sessions.GetOrCreate(shopperId);
            sessions.Save(shopperId, current with { ShownProductIds = shown.ToList() });
            if (unique.Count != result.Outfits.Count)
                return result with { Outfits = unique };
        }

        return result;
    }

    private async Task<StylistResult> AssembleFromPlanAsync(StylistIntent intent, GeminiOutfitPlan plan, HashSet<string> shown, CancellationToken ct)
    {
        var outfits = new List<ProductSummaryDto>();
        var picked = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
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
                Limit: 5), ct);
            var pick = page.Items.FirstOrDefault(i => !shown.Contains(i.ProductId) && !picked.Contains(i.ProductId))
                ?? page.Items.FirstOrDefault(i => !picked.Contains(i.ProductId))
                ?? page.Items.FirstOrDefault();
            if (pick is not null)
            {
                outfits.Add(pick);
                shown.Add(pick.ProductId);
                picked.Add(pick.ProductId);
            }
        }

        return new StylistResult("ready", "Assembling your outfits", intent, outfits, CatalogCalled: true, Plan: plan);
    }

    private async Task<StylistResult> AssembleAsync(StylistIntent intent, HashSet<string> shown, CancellationToken ct)
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
            Limit: 20);

        var page = await products.ListAsync(query, ct);
        var fresh = page.Items.Where(i => !shown.Contains(i.ProductId)).Take(3).ToList();
        var outfits = fresh.Count > 0 ? fresh : page.Items.Take(3).ToList();
        return new StylistResult(
            "ready",
            "Assembling your outfits",
            intent,
            outfits,
            CatalogCalled: true);
    }
}
