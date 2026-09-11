using OneFit.Application.Features.Products;

namespace OneFit.Api.Endpoints;

public static class ProductEndpoints
{
    public static void MapProducts(this WebApplication app)
    {
        var group = app.MapGroup("/products").WithTags("Products");

        group.MapGet("/", async (
            string? category,
            decimal? max_price_egp,
            string? q,
            bool? in_stock_only,
            string? sort,
            int? page,
            int? page_size,
            IProductQueryService service,
            CancellationToken ct) =>
        {
            var parsed = ProductListParser.TryParse(
                category, max_price_egp, q, in_stock_only, sort, page, page_size);
            if (parsed.Error is not null)
                return Results.BadRequest(new { error = parsed.Error });

            var res = await service.ListAsync(parsed.Query!, ct);
            return Results.Ok(res);
        })
        .WithName("ListProducts")
        .WithSummary("Public paginated product list with filters, search and sort.");

        group.MapGet("/{id}", async (string id, IProductQueryService service, CancellationToken ct) =>
        {
            var product = await service.GetByIdAsync(id, ct);
            return product is null
                ? Results.NotFound(new { error = "product not found" })
                : Results.Ok(product);
        })
        .WithName("GetProductById")
        .WithSummary("Public product detail with per-size stock.");
    }
}
