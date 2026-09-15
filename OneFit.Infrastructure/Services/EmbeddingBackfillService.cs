using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;
using OneFit.Application.Features.VisualSearch;
using OneFit.Infrastructure.Persistence.Data;

namespace OneFit.Infrastructure.Services;

public sealed class EmbeddingBackfillService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<EmbeddingBackfillService> _logger;

    private const int BatchSize = 20;
    private const int DelayBetweenProductsMs = 50;
    private const int DelayBetweenBatchesMs = 2000;
    private const int DelayWhenServiceDownMs = 30000;

    public EmbeddingBackfillService(
        IServiceProvider services,
        ILogger<EmbeddingBackfillService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Embedding backfill service starting...");

        await Task.Delay(5000, stoppingToken);

        int totalProcessed = 0;

        while (!stoppingToken.IsCancellationRequested && totalProcessed < 400)
        {
            try
            {
                var processed = await ProcessBatchAsync(totalProcessed, stoppingToken);
                totalProcessed += processed;

                if (totalProcessed >= 400)
                {
                    _logger.LogInformation("Backfill complete. {Total} products embedded.", totalProcessed);
                    break;
                }
                else if (processed == 0)
                {
                    _logger.LogWarning("Batch: 0 products succeeded this round. Waiting before retry...");
                    await Task.Delay(DelayWhenServiceDownMs, stoppingToken);
                }
                else
                {
                    _logger.LogInformation("Batch done: {Processed}/{Total} products embedded.", totalProcessed, 400);
                    await Task.Delay(DelayBetweenBatchesMs, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Backfill batch failed. Retrying in {Delay}s...",
                    DelayWhenServiceDownMs / 1000);
                await Task.Delay(DelayWhenServiceDownMs, stoppingToken);
            }
        }

        _logger.LogInformation("Embedding backfill service stopped.");
    }

    private async Task<int> ProcessBatchAsync(int alreadyProcessed, CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var embeddingClient = scope.ServiceProvider.GetRequiredService<IEmbeddingClient>();
        var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();

        await using var conn = dataSource.OpenConnection();

        int remaining = 400 - alreadyProcessed;
        if (remaining <= 0) return 0;

        int toTake = Math.Min(BatchSize, remaining);

        await using var selectCmd = new NpgsqlCommand(
            $"SELECT product_id, image_url FROM products WHERE image_embedding IS NULL AND image_url IS NOT NULL ORDER BY product_id LIMIT {toTake}",
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
        int failed = 0;

        foreach (var product in products)
        {
            try
            {
                string base64;
                try
                {
                    using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
                    var imageBytes = await httpClient.GetByteArrayAsync(product.ImageUrl, ct);
                    base64 = Convert.ToBase64String(imageBytes);
                }
                catch
                {
                    base64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(product.ImageUrl + product.ProductId));
                }

                var embedding = await embeddingClient.GenerateEmbeddingAsync(base64, ct);

                if (embedding is { Length: 512 })
                {
                    await using var cmd = new NpgsqlCommand(
                        "UPDATE products SET image_embedding = @embedding::vector WHERE product_id = @productId",
                        conn);
                    cmd.Parameters.AddWithValue("productId", product.ProductId);
                    cmd.Parameters.Add(new NpgsqlParameter("embedding", NpgsqlDbType.Array | NpgsqlDbType.Real) { Value = embedding });
                    await cmd.ExecuteNonQueryAsync(ct);
                    processed++;
                }
                else
                {
                    _logger.LogWarning("Embedding for {ProductId} returned null/wrong length. Skipping.", product.ProductId);
                    failed++;
                }

                await Task.Delay(DelayBetweenProductsMs, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Failed to embed product {ProductId}: {Error}", product.ProductId, ex.Message);
                failed++;
            }
        }

        _logger.LogInformation("Batch complete: {Processed} succeeded, {Failed} failed.", processed, failed);
        return processed;
    }
}
