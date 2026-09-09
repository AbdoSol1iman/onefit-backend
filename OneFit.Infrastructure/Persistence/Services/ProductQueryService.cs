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

        var items = await ApplySort(baseQuery, query.Sort)
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

        return new PagedResult<ProductSummaryDto>(items, query.Page, query.PageSize, total);
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

        if (filter.InStockOnly)
            query = query.Where(p => p.ProductSizes.Any(s => s.StockQty > 0));

        return query;
    }

    private static IQueryable<Product> ApplySort(IQueryable<Product> query, string sort) =>
        sort switch
        {
            "price_desc" => query.OrderByDescending(p => p.PriceEgp).ThenBy(p => p.ProductId),
            "newest" => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.ProductId),
            _ => query.OrderBy(p => p.PriceEgp).ThenBy(p => p.ProductId),
        };

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
