using OneFit.Application.Features.Products;

namespace OneFit.Api.Endpoints;

public static class ProductEndpoints
{
    private static readonly HashSet<string> Sorts = ["price_asc", "price_desc", "newest"];

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
            sort = string.IsNullOrWhiteSpace(sort) ? "price_asc" : sort.Trim().ToLower();
            var inStock = in_stock_only ?? true;
            var pageValue = page ?? 1;
            var pageSizeValue = page_size ?? 20;
            if (!Sorts.Contains(sort))
                return Results.BadRequest(new { error = "sort must be price_asc, price_desc or newest" });
            if (pageValue is < 1)
                return Results.BadRequest(new { error = "page must be >= 1" });
            if (pageSizeValue is < 1 or > 50)
                return Results.BadRequest(new { error = "page_size must be 1..50" });
            if (max_price_egp is < 0)
                return Results.BadRequest(new { error = "max_price_egp must be >= 0" });

            var res = await service.ListAsync(new ProductListQuery(
                category, max_price_egp, q, inStock, sort, pageValue, pageSizeValue), ct);
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
