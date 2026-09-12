using Microsoft.EntityFrameworkCore;
using OneFit.Application.Features.Products;
using OneFit.Domain.Entities;
using OneFit.Infrastructure.Persistence.Data;

namespace OneFit.Infrastructure.Persistence.Services;

public sealed class ProductQueryService(OneFitDbContext db) : IProductQueryService
{
    public async Task<PagedResult<ProductSummaryDto>> ListAsync(ProductListQuery query, CancellationToken ct = default)
    {
        var baseQuery = ApplyFilters(db.Products.AsNoTracking(), query);

        var total = await baseQuery.CountAsync(ct);

        if (query.Limit.HasValue)
        {
            var take = query.Limit.Value;
            var limited = await FetchAsync(baseQuery, query, take, 0, ct);
            return new PagedResult<ProductSummaryDto>(limited, 1, take, total);
        }

        if (HasRankTags(query))
        {
            var ranked = await FetchRankedAsync(baseQuery, query, 50, ct);
            var items = ranked
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .Select(r => new ProductSummaryDto(
                    r.ProductId,
                    r.Brand,
                    r.Name,
                    r.PriceEgp,
                    r.Sizes,
                    r.ImageUrl))
                .ToList();
            return new PagedResult<ProductSummaryDto>(items, query.Page, query.PageSize, total);
        }

        var pageItems = await ApplySort(baseQuery, query.Sort)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductSummaryDto(
                p.ProductId,
                p.Brand.Name,
                p.Name,
                p.PriceEgp,
                p.ProductSizes.Where(s => s.StockQty > 0).OrderBy(s => s.Size).Select(s => s.Size).ToList(),
                p.ImageUrl))
            .ToListAsync(ct);

        return new PagedResult<ProductSummaryDto>(pageItems, query.Page, query.PageSize, total);
    }

    public async Task<ProductDetailDto?> GetByIdAsync(string productId, CancellationToken ct = default)
    {
        return await db.Products
            .AsNoTracking()
            .Where(p => p.ProductId == productId)
            .Select(p => new ProductDetailDto(
                p.ProductId,
                p.Brand.Name,
                p.BrandId,
                p.Name,
                p.Category,
                p.PriceEgp,
                p.StyleTags,
                p.ProductSizes.OrderBy(s => s.Size).Select(s => new ProductSizeDto(s.Size, s.StockQty)).ToList(),
                p.ImageUrl))
            .SingleOrDefaultAsync(ct);
    }

    private static IQueryable<Product> ApplyFilters(IQueryable<Product> query, ProductListQuery filter)
    {
        if (!string.IsNullOrWhiteSpace(filter.Category))
            query = query.Where(p => p.Category == filter.Category!.Trim().ToLower());

        if (filter.MaxPriceEgp.HasValue)
            query = query.Where(p => p.PriceEgp <= filter.MaxPriceEgp.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = $"%{EscapeLike(filter.Search.Trim())}%";
            query = query.Where(p => EF.Functions.ILike(p.Name, pattern));
        }

        var normalized = NormalizeTags(filter.StyleTags);
        if (normalized is { Count: > 0 })
        {
            if (filter.StyleMatch == "all")
                query = query.Where(p =>
                    p.StyleTags != null &&
                    normalized.All(tag => p.StyleTags.Contains(tag)));
            else if (filter.StyleMatch == "any")
                query = query.Where(p =>
                    p.StyleTags != null &&
                    normalized.Any(tag => p.StyleTags.Contains(tag)));
        }

        if (filter.InStockOnly)
            query = query.Where(p => p.ProductSizes.Any(s => s.StockQty > 0));

        return query;
    }

    private static bool HasRankTags(ProductListQuery query) =>
        query.StyleMatch == "rank" && query.StyleTags is { Count: > 0 };

    private static List<string>? NormalizeTags(List<string>? tags)
    {
        if (tags is null || tags.Count == 0)
            return null;
        var normalized = tags
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLower())
            .Distinct()
            .ToList();
        return normalized.Count == 0 ? null : normalized;
    }

    private sealed record RankedCandidate(
        string ProductId,
        string Brand,
        string Name,
        decimal PriceEgp,
        string? ImageUrl,
        List<string>? StyleTags,
        List<string> Sizes);

    private static async Task<List<ProductSummaryDto>> FetchAsync(
        IQueryable<Product> baseQuery, ProductListQuery query, int take, int skip, CancellationToken ct)
    {
        if (HasRankTags(query))
        {
            var ranked = await FetchRankedAsync(baseQuery, query, 50, ct);
            return ranked.Skip(skip).Take(take)
                .Select(r => new ProductSummaryDto(r.ProductId, r.Brand, r.Name, r.PriceEgp, r.Sizes, r.ImageUrl))
                .ToList();
        }

        return await ApplySort(baseQuery, query.Sort)
            .Skip(skip)
            .Take(take)
            .Select(p => new ProductSummaryDto(
                p.ProductId,
                p.Brand.Name,
                p.Name,
                p.PriceEgp,
                p.ProductSizes.Where(s => s.StockQty > 0).OrderBy(s => s.Size).Select(s => s.Size).ToList(),
                p.ImageUrl))
            .ToListAsync(ct);
    }

    private static async Task<List<RankedCandidate>> FetchRankedAsync(
        IQueryable<Product> baseQuery, ProductListQuery query, int candidateLimit, CancellationToken ct)
    {
        var tagSet = query.StyleTags!.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var candidates = await ApplySort(baseQuery, query.Sort)
            .Take(candidateLimit)
            .Select(p => new RankedCandidate(
                p.ProductId,
                p.Brand.Name,
                p.Name,
                p.PriceEgp,
                p.ImageUrl,
                p.StyleTags,
                p.ProductSizes.Where(s => s.StockQty > 0).OrderBy(s => s.Size).Select(s => s.Size).ToList()))
            .ToListAsync(ct);

        return candidates
            .OrderByDescending(r => r.StyleTags == null ? 0 : r.StyleTags.Count(t => tagSet.Contains(t)))
            .ToList();
    }

    private static readonly Dictionary<string, Func<IQueryable<Product>, IOrderedQueryable<Product>>> Sorts = new()
    {
        ["price_desc"] = q => q.OrderByDescending(p => p.PriceEgp).ThenBy(p => p.ProductId),
        ["newest"] = q => q.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.ProductId),
        ["price_asc"] = q => q.OrderBy(p => p.PriceEgp).ThenBy(p => p.ProductId),
    };

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, string sort) =>
        Sorts.TryGetValue(sort, out var apply) ? apply(query) : Sorts["price_asc"](query);

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
