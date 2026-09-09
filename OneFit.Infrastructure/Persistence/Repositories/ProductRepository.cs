// OneFit.Infrastructure/Persistence/Repositories/ProductRepository.cs

using Microsoft.EntityFrameworkCore;
using OneFit.Application.Common.Interfaces.Catalog;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Domain.Entities;
using OneFit.Infrastructure.Persistence.Data;

namespace OneFit.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly OneFitDbContext _db;

    public ProductRepository(OneFitDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<Product>> QueryAsync(
        CatalogFilter filter,
        CancellationToken ct = default)
    {
        var query = _db.Products
            .AsNoTracking()
            .Include(p => p.Brand)
            .Include(p => p.ProductSizes)
            .Where(p => p.Category == filter.Category);

        // ============================================
        // Max Price Filter
        // ============================================

        if (filter.MaxPriceEgp is { } maxPrice)
        {
            query = query.Where(p => p.PriceEgp <= maxPrice);
        }

        // ============================================
        // Style Tags Filter - AND
        // Product must contain ALL requested tags
        // ============================================

        if (filter.StyleTags is { Count: > 0 } tags)
        {
            var normalizedTags = tags
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .Select(t => t.Trim().ToLower())
                .Distinct()
                .ToList();

            if (normalizedTags.Count > 0)
            {
                query = query.Where(p =>
                    p.StyleTags != null &&
                    normalizedTags.All(tag =>
                        p.StyleTags.Contains(tag)));
            }
        }

        // ============================================
        // In Stock Filter
        // ============================================

        if (filter.InStockOnly)
        {
            query = query.Where(p =>
                p.ProductSizes.Any(s => s.StockQty > 0));
        }

        // ============================================
        // Ordering + Limit
        // ============================================

        return await query
            .OrderBy(p => p.PriceEgp)
            .Take(filter.Limit)
            .ToListAsync(ct);
    }
}