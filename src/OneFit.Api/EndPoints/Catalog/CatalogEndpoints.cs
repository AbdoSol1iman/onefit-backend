
using MediatR;
using OneFit.Application.Features.Catalog.Models;
using OneFit.Application.Features.Catalog.Queries.QueryCatalog;

namespace OneFit.Api.Endpoints;

public static class CatalogEndpoints
{
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/v1/catalog/query", async (CatalogQueryRequest request, ISender sender, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.Category))
            {
                return Results.BadRequest(new
                {
                    error = new { code = "INVALID_REQUEST", message = "category is required." }
                });
            }

            var query = new QueryCatalogQuery(
                Category: request.Category,
                MaxPriceEgp: request.MaxPriceEgp,
                StyleTags: request.StyleTags,
                InStockOnly: request.InStockOnly,
                Limit: request.Limit <= 0 ? 3 : request.Limit);

            var response = await sender.Send(query, ct);
            return Results.Ok(response);
        })
        .WithName("QueryCatalog")
        .Produces<CatalogQueryResponseDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}