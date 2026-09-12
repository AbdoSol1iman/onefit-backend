using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneFit.Api.Endpoints.Shared;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Features.WishList.Commands.AddWishlistItem;
using OneFit.Application.Features.WishList.Commands.RemoveWishlistItem;
using OneFit.Application.Features.WishList.Queries.GetWishlist;

namespace OneFit.Api.EndPoints.WishList
{
    /// <summary>
    /// Wishlist endpoints for managing wishlist operations
    /// </summary>
    public static class WishlistEndpoints
    {
        /// <summary>
        /// Maps all wishlist-related API endpoints
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - POST /api/v1/wishlist/items - Add item to wishlist
        /// - DELETE /api/v1/wishlist/items/{wishlist_item_id} - Remove item from wishlist
        /// - GET /api/v1/wishlist - Get wishlist items for a shopper
        /// </remarks>
        public static void MapWishlistEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/wishlist")
                .WithTags("Wishlist")
                .WithOpenApi();

            // Add item to wishlist
            group.MapPost("/items", AddWishlistItemHandler)
                .WithName("AddWishlistItem")
                .WithSummary("Add item to wishlist")
                .WithDescription("Adds a new item to the wishlist")
                .Produces(200)
                .Produces(400)
                .Produces(404);

            // Remove item from wishlist
            group.MapDelete("/items/{wishlist_item_id}", RemoveWishlistItemHandler)
                .WithName("RemoveWishlistItem")
                .WithSummary("Remove item from wishlist")
                .WithDescription("Removes an item from the wishlist")
                .Produces(200)
                .Produces(400)
                .Produces(404);

            // Get wishlist items
            group.MapGet("", GetWishlistHandler)
                .WithName("GetWishlist")
                .WithSummary("Get wishlist items")
                .WithDescription("Retrieves all wishlist items for a specific shopper")
                .Produces(200)
                .Produces(400);
        }

        /// <summary>
        /// Handles adding an item to the wishlist
        /// </summary>
        private static async Task<IResult> AddWishlistItemHandler(
            AddWishlistItemRequest request,
            ISender sender,
            CancellationToken ct)
        {
            try
            {
                var command = new AddWishlistItemCommand(
                    request.ShopperId,
                    request.ProductId,
                    request.Size);

                var result = await sender.Send(command, ct);
                return Results.Ok(new
                {
                    item = result
                });
            }
            catch (NotFoundException ex)
            {
                return EndpointHelpers.FromNotFoundException(ex);
            }
            catch (Exception)
            {
                return EndpointHelpers.InternalError();
            }
        }

        /// <summary>
        /// Handles removing an item from the wishlist
        /// </summary>
        private static async Task<IResult> RemoveWishlistItemHandler(
            string wishlist_item_id,
            [FromHeader(Name = "X-Shopper-Id")] string shopperId,
            ISender sender,
            CancellationToken ct)
        {
            var invalid =
                EndpointHelpers.Require(shopperId, "X-Shopper-Id", "X-Shopper-Id") ??
                EndpointHelpers.Require(wishlist_item_id, "wishlist_item_id");
            if (invalid is not null)
                return invalid;

            try
            {
                var result = await sender.Send(
                    new RemoveWishlistItemCommand(shopperId, wishlist_item_id),
                    ct);

                return Results.Ok(new
                {
                    deleted = result
                });
            }
            catch (NotFoundException ex)
            {
                return EndpointHelpers.FromNotFoundException(ex);
            }
            catch (Exception)
            {
                return EndpointHelpers.InternalError();
            }
        }

        /// <summary>
        /// Handles retrieving wishlist items for a shopper
        /// </summary>
        private static async Task<IResult> GetWishlistHandler(
            [FromQuery(Name = "shopper_id")] string shopperId,
            ISender sender,
            CancellationToken ct)
        {
            var invalid = EndpointHelpers.Require(shopperId, "shopper_id query parameter");
            if (invalid is not null)
                return invalid;

            try
            {
                var items = await sender.Send(new GetWishlistQuery(shopperId), ct);
                return Results.Ok(new
                {
                    items = items
                });
            }
            catch (Exception)
            {
                return EndpointHelpers.InternalError();
            }
        }
    }
}
