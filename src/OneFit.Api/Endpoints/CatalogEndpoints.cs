using OneFit.Application.Features.Catalog;

namespace OneFit.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalog(this WebApplication app)
    {
        app.MapPost("/catalog/query", async (
            QueryCatalogRequest req,
            ICatalogQueryService service,
            CancellationToken ct) =>
        {
            if (req.Limit is < 1 or > 20)
                return Results.BadRequest(new { error = "limit must be 1..20" });
            if (req.MaxPriceEgp is < 0)
                return Results.BadRequest(new { error = "max_price_egp must be >= 0" });

            var res = await service.QueryAsync(req, ct);
            return Results.Ok(res);
        })
        .WithName("QueryCatalog")
        .WithSummary("Internal catalog query (Dev A Day-1). Also usable by frontend for direct search.");
    }
}
