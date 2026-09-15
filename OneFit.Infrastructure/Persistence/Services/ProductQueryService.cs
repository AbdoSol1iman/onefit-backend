using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OneFit.Application.Features.Products;
using OneFit.Domain.Entities;
using OneFit.Infrastructure.Persistence.Data;

namespace OneFit.Infrastructure.Persistence.Services;

public sealed class ProductQueryService(OneFitDbContext db) : IProductQueryService
{
    public async Task<PagedResult<ProductSummaryDto>> ListAsync(ProductListQuery query, CancellationToken ct = default)
    {
        var conn = (NpgsqlConnection)db.Database.GetDbConnection();
        await conn.OpenAsync(ct);

        var where = new StringBuilder("WHERE p.image_embedding IS NOT NULL");
        var parameters = new List<NpgsqlParameter>();

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            where.Append(" AND p.category = @category");
            parameters.Add(new("category", query.Category.Trim().ToLower()));
        }

        if (query.MaxPriceEgp.HasValue)
        {
            where.Append(" AND p.price_egp <= @maxPrice");
            parameters.Add(new("maxPrice", query.MaxPriceEgp.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            where.Append(" AND p.name ILIKE @search");
            parameters.Add(new("search", $"%{EscapeLike(query.Search.Trim())}%"));
        }

        if (query.InStockOnly)
        {
            where.Append(" AND EXISTS (SELECT 1 FROM product_sizes ps WHERE ps.product_id = p.product_id AND ps.stock_qty > 0)");
        }

        var orderClause = query.Sort switch
        {
            "price_desc" => "ORDER BY p.price_egp DESC, p.product_id",
            "newest" => "ORDER BY p.created_at DESC, p.product_id",
            _ => "ORDER BY p.price_egp ASC, p.product_id"
        };

        var countSql = $"SELECT COUNT(*) FROM products p {where}";
        await using var countCmd = new NpgsqlCommand(countSql, conn);
        countCmd.Parameters.AddRange(parameters.ToArray());
        var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct));

        var offset = (query.Page - 1) * query.PageSize;
        var dataSql = $"""
            SELECT p.product_id, b.name AS brand, p.name, p.category, p.price_egp, p.image_url,
                   ARRAY_TO_STRING(ARRAY(
                       SELECT ps.size FROM product_sizes ps
                       WHERE ps.product_id = p.product_id AND ps.stock_qty > 0
                       ORDER BY ps.size
                   ), ',') AS sizes
            FROM products p
            JOIN brands b ON b.brand_id = p.brand_id
            {where}
            {orderClause}
            OFFSET @offset LIMIT @limit
        """;

        await using var dataCmd = new NpgsqlCommand(dataSql, conn);
        dataCmd.Parameters.AddRange(parameters.ToArray());
        dataCmd.Parameters.Add(new("offset", offset));
        dataCmd.Parameters.Add(new("limit", query.PageSize));

        var items = new List<ProductSummaryDto>();
        await using var reader = await dataCmd.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            var sizesStr = reader.IsDBNull(6) ? "" : reader.GetString(6);
            var sizes = string.IsNullOrEmpty(sizesStr)
                ? new List<string>()
                : sizesStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            items.Add(new ProductSummaryDto(
                reader.GetString(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetString(3),
                reader.GetDecimal(4),
                sizes,
                reader.IsDBNull(5) ? null : reader.GetString(5)));
        }

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

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_");
}
