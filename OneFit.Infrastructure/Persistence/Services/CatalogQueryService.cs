using Microsoft.EntityFrameworkCore;
using OneFit.Application.Features.Catalog;
using OneFit.Infrastructure.Persistence.Data;

namespace OneFit.Infrastructure.Persistence.Services;

public sealed class CatalogQueryService(OneFitDbContext db) : ICatalogQueryService
{
    public async Task<QueryCatalogResponse> QueryAsync(QueryCatalogRequest request, CancellationToken ct = default)
    {
        var limit = Math.Clamp(request.Limit <= 0 ? 3 : request.Limit, 1, 20);
        var tags = request.StyleTags?
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t.Trim().ToLower())
            .ToList() ?? [];

        var query = db.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.ProductSizes)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Category))
            query = query.Where(p => p.Category == request.Category!.Trim().ToLower());

        if (request.MaxPriceEgp.HasValue)
            query = query.Where(p => p.PriceEgp <= request.MaxPriceEgp.Value);

        if (request.InStockOnly)
            query = query.Where(p => p.ProductSizes.Any(s => s.StockQty > 0));

        var rows = await query
            .OrderBy(p => p.PriceEgp)
            .Take(50)
            .Select(p => new
            {
                p.ProductId,
                BrandName = p.Brand.Name,
                p.Name,
                p.PriceEgp,
                p.ImageUrl,
                p.StyleTags,
                Sizes = p.ProductSizes.Where(s => s.StockQty > 0).Select(s => s.Size).ToList()
            })
            .ToListAsync(ct);

        if (tags.Count > 0)
            rows = rows
                .OrderByDescending(r => r.StyleTags == null ? 0 : r.StyleTags.Count(t => tags.Contains(t.ToLower())))
                .ToList();

        var results = rows.Take(limit).Select(r => new CatalogItemDto(
            r.ProductId, r.BrandName, r.Name, r.PriceEgp, r.Sizes, r.ImageUrl)).ToList();

        return new QueryCatalogResponse(results);
    }
}
