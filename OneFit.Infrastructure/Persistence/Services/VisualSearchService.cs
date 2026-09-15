using System.Text;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;
using OneFit.Application.Features.Products;
using OneFit.Application.Features.VisualSearch;
using OneFit.Infrastructure.Persistence.Data;

namespace OneFit.Infrastructure.Persistence.Services;

public sealed class VisualSearchService : IVisualSearchService
{
    private readonly OneFitDbContext _db;
    private readonly IEmbeddingClient _embeddingClient;
    private readonly NpgsqlDataSource _dataSource;

    public VisualSearchService(OneFitDbContext db, IEmbeddingClient embeddingClient, NpgsqlDataSource dataSource)
    {
        _db = db;
        _embeddingClient = embeddingClient;
        _dataSource = dataSource;
    }

    public async Task<VisualSearchResponse> SearchAsync(
        float[] queryEmbedding,
        string? category = null,
        decimal? maxPriceEgp = null,
        bool? inStockOnly = null,
        int topN = 10,
        CancellationToken ct = default)
    {
        var sql = new StringBuilder();
        sql.AppendLine("""
            SELECT p.product_id, b.name AS brand, p.name, p.category, p.price_egp, p.image_url,
                   1 - (p.image_embedding <=> @embedding::vector) AS similarity,
                   ARRAY_TO_STRING(ARRAY(
                       SELECT ps.size FROM product_sizes ps
                       WHERE ps.product_id = p.product_id AND ps.stock_qty > 0
                       ORDER BY ps.size
                   ), ',') AS sizes
            FROM products p
            JOIN brands b ON b.brand_id = p.brand_id
            WHERE p.image_embedding IS NOT NULL
        """);

        var parameters = new List<NpgsqlParameter>
        {
            new("embedding", queryEmbedding) { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Real }
        };

        if (!string.IsNullOrWhiteSpace(category))
        {
            sql.AppendLine("AND p.category = @category");
            parameters.Add(new("category", category.Trim().ToLower()));
        }

        if (maxPriceEgp.HasValue)
        {
            sql.AppendLine("AND p.price_egp <= @maxPrice");
            parameters.Add(new("maxPrice", maxPriceEgp.Value));
        }

        if (inStockOnly == true)
        {
            sql.AppendLine("AND EXISTS (SELECT 1 FROM product_sizes ps WHERE ps.product_id = p.product_id AND ps.stock_qty > 0)");
        }

        sql.AppendLine("ORDER BY p.image_embedding <=> @embedding::vector");
        sql.AppendLine("LIMIT @topN");
        parameters.Add(new("topN", topN));

        var results = new List<VisualSearchResult>();

        await using var conn = _dataSource.OpenConnection();

        await using var cmd = new NpgsqlCommand(sql.ToString(), conn);
        cmd.Parameters.AddRange(parameters.ToArray());

        await using var reader = await cmd.ExecuteReaderAsync(ct);

        while (await reader.ReadAsync(ct))
        {
            var productId = reader.GetString(0);
            var brand = reader.GetString(1);
            var name = reader.GetString(2);
            var productCategory = reader.GetString(3);
            var priceEgp = reader.GetDecimal(4);
            var imageUrl = reader.IsDBNull(5) ? null : reader.GetString(5);
            var similarity = reader.GetDouble(6);
            var sizesStr = reader.IsDBNull(7) ? "" : reader.GetString(7);
            var sizes = string.IsNullOrEmpty(sizesStr)
                ? new List<string>()
                : sizesStr.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

            var productDto = new ProductSummaryDto(productId, brand, name, productCategory, priceEgp, sizes, imageUrl);
            results.Add(new VisualSearchResult(productDto, similarity));
        }

        return new VisualSearchResponse(results, results.Count);
    }

    public async Task<int> BackfillEmbeddingsAsync(
        Func<int, int, Task>? onProgress = null,
        CancellationToken ct = default)
    {
        await using var conn = _dataSource.OpenConnection();

        await using var countCmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM products WHERE image_embedding IS NULL AND image_url IS NOT NULL",
            conn);
        var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync(ct));

        await using var selectCmd = new NpgsqlCommand(
            "SELECT product_id, image_url FROM products WHERE image_embedding IS NULL AND image_url IS NOT NULL",
            conn);

        var products = new List<(string ProductId, string ImageUrl)>();
        await using (var reader = await selectCmd.ExecuteReaderAsync(ct))
        {
            while (await reader.ReadAsync(ct))
            {
                products.Add((reader.GetString(0), reader.GetString(1)));
            }
        }

        int processed = 0;

        foreach (var product in products)
        {
            try
            {
                var imageBytes = await DownloadImageAsBytesAsync(product.ImageUrl!, ct);
                if (imageBytes is null)
                    continue;

                var base64 = Convert.ToBase64String(imageBytes);
                var embedding = await _embeddingClient.GenerateEmbeddingAsync(base64, ct);

                if (embedding is { Length: 512 })
                {
                    await UpdateProductEmbeddingAsync(product.ProductId, embedding, ct);
                }

                processed++;

                if (onProgress is not null)
                    await onProgress(processed, total);

                await Task.Delay(100, ct);
            }
            catch
            {
                // Skip products that fail embedding generation
            }
        }

        return processed;
    }

    private async Task UpdateProductEmbeddingAsync(string productId, float[] embedding, CancellationToken ct)
    {
        var sql = "UPDATE products SET image_embedding = @embedding::vector WHERE product_id = @productId";

        await using var conn = _dataSource.OpenConnection();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("productId", productId);
        cmd.Parameters.Add(new NpgsqlParameter("embedding", NpgsqlDbType.Array | NpgsqlDbType.Real) { Value = embedding });

        await cmd.ExecuteNonQueryAsync(ct);
    }

    private static async Task<byte[]?> DownloadImageAsBytesAsync(string imageUrl, CancellationToken ct)
    {
        using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
        return await httpClient.GetByteArrayAsync(imageUrl, ct);
    }
}
