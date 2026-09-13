using Microsoft.EntityFrameworkCore;
using OneFit.Application.Features.Feed;
using OneFit.Domain.Entities;
using OneFit.Infrastructure.Persistence.Data;

namespace OneFit.Infrastructure.Persistence.Services;

public sealed class FeedService(OneFitDbContext db) : IFeedService
{
    private const int Lookback = 50;
    private const int ColdStartThreshold = 5;

    public Task<bool> ProductExistsAsync(string productId, CancellationToken ct = default) =>
        db.Products.AsNoTracking().AnyAsync(p => p.ProductId == productId, ct);

    public async Task RecordAsync(string shopperId, string productId, string interactionType, CancellationToken ct = default)
    {
        db.UserInteractions.Add(new UserInteraction
        {
            InteractionId = Guid.NewGuid().ToString(),
            ShopperId = shopperId,
            ProductId = productId,
            InteractionType = interactionType,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<FeedItemDto>> GetFeedAsync(string shopperId, int limit, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 50);

        var total = await db.UserInteractions
            .Where(i => i.ShopperId == shopperId)
            .CountAsync(ct);

        if (total < ColdStartThreshold)
            return await ColdStartAsync(new HashSet<string>(), limit, ct);

        var recent = await db.UserInteractions
            .AsNoTracking()
            .Where(i => i.ShopperId == shopperId)
            .OrderByDescending(i => i.CreatedAt)
            .Take(Lookback)
            .Join(db.Products.AsNoTracking(),
                i => i.ProductId,
                p => p.ProductId,
                (i, p) => new { i.InteractionType, p.Category, p.BrandId })
            .ToListAsync(ct);

        if (recent.Count == 0)
            return await ColdStartAsync(new HashSet<string>(), limit, ct);

        static int Weight(string t) => t == "save" ? 5 : t == "like" ? 3 : 1;

        var topCategory = recent
            .GroupBy(x => x.Category)
            .Select(g => new { Category = g.Key, Score = g.Sum(x => Weight(x.InteractionType)) })
            .OrderByDescending(g => g.Score)
            .First().Category;

        var topBrandId = recent
            .GroupBy(x => x.BrandId)
            .Select(g => new { BrandId = g.Key, Score = g.Sum(x => Weight(x.InteractionType)) })
            .OrderByDescending(g => g.Score)
            .First().BrandId;

        var seen = new HashSet<string>(await db.UserInteractions
            .AsNoTracking()
            .Where(i => i.ShopperId == shopperId)
            .Select(i => i.ProductId)
            .Distinct()
            .ToListAsync(ct));

        var items = await db.Products
            .AsNoTracking()
            .Where(p => !seen.Contains(p.ProductId))
            .Where(p => p.ProductSizes.Any(s => s.StockQty > 0))
            .OrderByDescending(p => p.Brand.IsLocal)
            .ThenByDescending(p => p.Category == topCategory)
            .ThenByDescending(p => p.BrandId == topBrandId)
            .ThenByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new FeedItemDto(
                p.ProductId,
                p.Name,
                p.Brand.Name,
                p.PriceEgp,
                p.ImageUrl))
            .ToListAsync(ct);

        if (items.Count < limit)
        {
            var taken = new HashSet<string>(seen);
            foreach (var item in items)
                taken.Add(item.ProductId);
            var filler = await ColdStartAsync(taken, limit - items.Count, ct);
            items.AddRange(filler);
        }

        return items;
    }

    private async Task<List<FeedItemDto>> ColdStartAsync(HashSet<string> exclude, int limit, CancellationToken ct)
    {
        return await db.Products
            .AsNoTracking()
            .Where(p => !exclude.Contains(p.ProductId))
            .Where(p => p.ProductSizes.Any(s => s.StockQty > 0))
            .OrderBy(p => p.BrandId)
            .ThenByDescending(p => p.CreatedAt)
            .Take(limit)
            .Select(p => new FeedItemDto(
                p.ProductId,
                p.Name,
                p.Brand.Name,
                p.PriceEgp,
                p.ImageUrl))
            .ToListAsync(ct);
    }
}
