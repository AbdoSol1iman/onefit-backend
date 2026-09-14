using OneFit.Application.Features.Products;
using OneFit.Application.Features.Stylist.Gemini;

namespace OneFit.Application.Features.Stylist;

public sealed record StylistIntent(
    string? Occasion = null,
    string? Setting = null,
    string? Style = null,
    decimal? BudgetEgp = null,
    bool BudgetSkipped = false
);

public sealed record StylistSession(
    StylistIntent Intent,
    bool AwaitingBudget = false
);

public sealed record StylistResult(
    string Status,
    string Reply,
    StylistIntent Intent,
    List<ProductSummaryDto> Outfits,
    bool CatalogCalled,
    GeminiOutfitPlan? Plan = null
);

public interface IStylistSessionStore
{
    StylistSession GetOrCreate(string shopperId);
    void Save(string shopperId, StylistSession session);
    void Reset(string shopperId);
}

public interface IStylistOrchestrator
{
    Task<StylistResult> HandleAsync(string shopperId, string message, bool newChat = false, CancellationToken ct = default);
}
