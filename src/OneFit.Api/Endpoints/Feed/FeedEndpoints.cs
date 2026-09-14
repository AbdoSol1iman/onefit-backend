using Microsoft.AspNetCore.Mvc;
using OneFit.Api.Endpoints.Shared;
using OneFit.Application.Features.Feed;

namespace OneFit.Api.Endpoints.Feed;

public sealed record RecordInteractionRequest(string ProductId, string InteractionType);

public static class FeedEndpoints
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "view", "like", "save"
    };

    public static void MapFeedEndpoints(this IEndpointRouteBuilder app)
    {
        var interactions = app.MapGroup("/api/v1/interactions").WithTags("Feed");
        interactions.MapPost("", RecordInteractionHandler)
            .WithName("RecordInteraction")
            .WithSummary("Record a shopper interaction")
            .Produces(200)
            .Produces(400)
            .Produces(404);

        var feed = app.MapGroup("/api/v1/feed").WithTags("Feed");
        feed.MapGet("", GetFeedHandler)
            .WithName("GetFeed")
            .WithSummary("Statistical recommendation feed, no ML model")
            .Produces(200)
            .Produces(400);
    }

    private static async Task<IResult> RecordInteractionHandler(
        RecordInteractionRequest request,
        [FromHeader(Name = "X-Shopper-Id")] string shopperId,
        IFeedService service,
        CancellationToken ct)
    {
        var invalid =
            EndpointHelpers.Require(shopperId, "X-Shopper-Id", "X-Shopper-Id") ??
            EndpointHelpers.Require(request.ProductId, "product_id") ??
            EndpointHelpers.Require(request.InteractionType, "interaction_type");
        if (invalid is not null)
            return invalid;

        var type = request.InteractionType.Trim().ToLowerInvariant();
        if (!AllowedTypes.Contains(type))
            return EndpointHelpers.BadRequest("INVALID_REQUEST", "interaction_type must be one of view, like, save.");

        if (!await service.ProductExistsAsync(request.ProductId, ct))
            return Results.NotFound(new { error = new { code = "PRODUCT_NOT_FOUND", message = $"No product '{request.ProductId}' was found." } });

        try
        {
            await service.RecordAsync(shopperId, request.ProductId, type, ct);
            return Results.Ok(new { recorded = true });
        }
        catch (Exception)
        {
            return EndpointHelpers.InternalError();
        }
    }

    private static async Task<IResult> GetFeedHandler(
        [FromQuery(Name = "shopper_id")] string shopperId,
        [FromQuery(Name = "limit")] int? limit,
        IFeedService service,
        CancellationToken ct)
    {
        var invalid = EndpointHelpers.Require(shopperId, "shopper_id query parameter");
        if (invalid is not null)
            return invalid;

        var take = limit ?? 20;
        if (take < 1 || take > 50)
            return EndpointHelpers.BadRequest("INVALID_REQUEST", "limit must be between 1 and 50.");

        try
        {
            var feed = await service.GetFeedAsync(shopperId, take, ct);
            return Results.Ok(new { feed });
        }
        catch (Exception)
        {
            return EndpointHelpers.InternalError();
        }
    }
}
