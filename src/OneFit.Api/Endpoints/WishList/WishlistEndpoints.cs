using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneFit.Api.Endpoints.Shared;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Features.WishList.Commands.AddWishlistItem;
using OneFit.Application.Features.WishList.Commands.RemoveWishlistItem;
using OneFit.Application.Features.WishList.Queries.GetWishlist;
using System.Security.Claims;

namespace OneFit.Api.Endpoints.WishList
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
                .RequireAuthorization();

            // Add item to wishlist
            group.MapPost("/items", AddWishlistItemHandler)
                .WithName("AddWishlistItem")
                .WithSummary("Add item to wishlist")
                .WithDescription("Adds a new item to the wishlist")
                .Produces(200)
                .Produces(400)
                .Produces(401)
                .Produces(404);

            // Remove item from wishlist
            group.MapDelete("/items/{wishlist_item_id}", RemoveWishlistItemHandler)
                .WithName("RemoveWishlistItem")
                .WithSummary("Remove item from wishlist")
                .WithDescription("Removes an item from the wishlist")
                .Produces(200)
                .Produces(400)
                .Produces(401)
                .Produces(404);

            // Get wishlist items
            group.MapGet("", GetWishlistHandler)
                .WithName("GetWishlist")
                .WithSummary("Get wishlist items")
                .WithDescription("Retrieves all wishlist items for the authenticated shopper")
                .Produces(200)
                .Produces(401);
        }

        /// <summary>
        /// Handles adding an item to the wishlist.
        /// Critical fix #4: ShopperId from JWT, not request body.
        /// </summary>
        private static async Task<IResult> AddWishlistItemHandler(
            AddWishlistItemRequest request,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct)
        {
            var shopperId = user.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(shopperId))
                return Results.Unauthorized();

            try
            {
                var command = new AddWishlistItemCommand(
                    shopperId,
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
        /// Handles removing an item from the wishlist.
        /// Critical fix #4: ShopperId from JWT, not X-Shopper-Id header.
        /// </summary>
        private static async Task<IResult> RemoveWishlistItemHandler(
            string wishlist_item_id,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct)
        {
            var shopperId = user.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(shopperId))
                return Results.Unauthorized();

            var invalid =
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
        /// Handles retrieving wishlist items for the authenticated shopper.
        /// Critical fix #4: ShopperId from JWT, not query parameter.
        /// </summary>
        private static async Task<IResult> GetWishlistHandler(
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct)
        {
            var shopperId = user.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(shopperId))
                return Results.Unauthorized();

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
