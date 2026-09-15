using OneFit.Application.Features.VisualSearch;

namespace OneFit.Api.Endpoints;

public static class VisualSearchEndpoints
{
    public static void MapVisualSearch(this WebApplication app)
    {
        var group = app.MapGroup("/visual-search").WithTags("Visual Search");

        group.MapPost("/", async (
            VisualSearchRequest request,
            IEmbeddingClient embeddingClient,
            IVisualSearchService visualSearchService,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.ImageBase64))
                return Results.BadRequest(new { error = new { code = "INVALID_REQUEST", message = "image_base64 is required." } });

            var embedding = await embeddingClient.GenerateEmbeddingAsync(request.ImageBase64, ct);
            if (embedding is null)
                return Results.BadRequest(new { error = new { code = "EMBEDDING_FAILED", message = "Failed to generate embedding from the provided image." } });

            var results = await visualSearchService.SearchAsync(
                embedding,
                request.Category,
                request.MaxPriceEgp,
                request.InStockOnly,
                topN: 10,
                ct);

            return Results.Ok(results);
        })
        .WithName("VisualSearch")
        .WithSummary("Find visually similar products by uploading an image. Returns the top 10 most similar products ranked by cosine similarity.");

        group.MapPost("/backfill-embeddings", async (
            IVisualSearchService visualSearchService,
            CancellationToken ct) =>
        {
            int processed = 0;
            int total = 0;

            var result = await visualSearchService.BackfillEmbeddingsAsync(
                async (done, t) =>
                {
                    processed = done;
                    total = t;
                },
                ct);

            return Results.Ok(new
            {
                processed,
                total,
                message = $"Embedding backfill completed. {processed}/{total} products processed."
            });
        })
        .WithName("BackfillEmbeddings")
        .WithSummary("One-time backfill: generate embeddings for all products that don't have one yet.");
    }
}
