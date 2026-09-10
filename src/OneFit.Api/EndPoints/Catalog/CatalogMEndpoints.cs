using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneFit.Application.Features.Catalog.Models;
using OneFit.Application.Features.Catalog.Queries.QueryCatalog;

namespace OneFit.Api.Endpoints;

/// <summary>
/// Catalog endpoints for browsing and searching products
/// </summary>
public static class CatalogMEndpoints
{
    /// <summary>
    /// Maps all catalog-related API endpoints
    /// </summary>
    public static void MapCatalogEndpoints(this IEndpointRouteBuilder app)
    {
        /// <summary>
        /// Query products from the catalog with filtering options
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// GET /api/v1/catalog/query?category=Electronics&maxPriceEgp=1000&inStockOnly=true&limit=20
        /// 
        /// Query Parameters:
        /// - category (required): Product category
        /// - maxPriceEgp (optional): Maximum price filter
        /// - styleTags (optional): Array of style tags
        /// - inStockOnly (optional): Only items in stock, default false
        /// - limit (optional): Number of results (1-100), default 10
        /// </remarks>
        app.MapGet("/api/v1/catalog/query", QueryCatalogHandler)
            .WithName("QueryCatalog")
            .WithSummary("Query catalog products")
            .WithDescription("Retrieves products from catalog with optional filtering by price, category, and availability")
            
            .Produces<CatalogQueryResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Handler for querying catalog products
    /// </summary>
    private static async Task<IResult> QueryCatalogHandler(
        [FromQuery] string category,
        [FromQuery(Name = "maxPriceEgp")] decimal? maxPriceEgp,
        [FromQuery(Name = "styleTags")] string? styleTagsJson,
        [FromQuery(Name = "inStockOnly")] bool inStockOnly = false,
        [FromQuery(Name = "limit")] int limit = 10,
        ISender sender = null!,
        CancellationToken ct = default)
    {
        try
        {
            // Parse styleTags from comma-separated string to list
            var styleTags = string.IsNullOrWhiteSpace(styleTagsJson)
                ? null
                : new List<string>(styleTagsJson.Split(',', StringSplitOptions.RemoveEmptyEntries));

            var request = new CatalogQueryRequest(
                Category: category,
                MaxPriceEgp: maxPriceEgp,
                StyleTags: styleTags,
                InStockOnly: inStockOnly,
                Limit: limit > 100 ? 100 : (limit <= 0 ? 10 : limit));

            var query = new QueryCatalogQuery(
                Category: request.Category,
                MaxPriceEgp: request.MaxPriceEgp,
                StyleTags: request.StyleTags,
                InStockOnly: request.InStockOnly,
                Limit: request.Limit);

            var response = await sender.Send(query, ct);
            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new
            {
                error = new { code = "INVALID_REQUEST", message = ex.Message }
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new
            {
                error = new { code = "INTERNAL_ERROR", message = ex.Message }
            });
        }
    }
}
