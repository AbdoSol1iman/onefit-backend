using OneFit.Application.Features.Products;

namespace OneFit.Application.Features.VisualSearch;

public sealed record VisualSearchRequest(
    string ImageBase64,
    string? Category = null,
    decimal? MaxPriceEgp = null,
    bool? InStockOnly = null
);

public sealed record VisualSearchResult(
    ProductSummaryDto Product,
    double Similarity
);

public sealed record VisualSearchResponse(
    List<VisualSearchResult> Results,
    int Total
);

public interface IEmbeddingClient
{
    Task<float[]?> GenerateEmbeddingAsync(string imageBase64, CancellationToken ct = default);
}

public interface IVisualSearchService
{
    Task<VisualSearchResponse> SearchAsync(
        float[] queryEmbedding,
        string? category = null,
        decimal? maxPriceEgp = null,
        bool? inStockOnly = null,
        int topN = 10,
        CancellationToken ct = default);

    Task<int> BackfillEmbeddingsAsync(
        Func<int, int, Task>? onProgress = null,
        CancellationToken ct = default);
}
