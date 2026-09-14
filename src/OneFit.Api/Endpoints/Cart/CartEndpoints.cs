using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneFit.Api.Endpoints.Shared;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Features.Cart.Commands.AddCartItem;
using OneFit.Application.Features.Cart.Commands.RemoveCartItem;
using System.Security.Claims;

namespace OneFit.Api.Endpoints.Cart
{
    /// <summary>
    /// Cart endpoints for managing shopping cart operations
    /// </summary>
    public static class CartEndpoints
    {
        /// <summary>
        /// Maps all cart-related API endpoints
        /// </summary>
        /// <remarks>
        /// Includes:
        /// - POST /api/v1/cart/items - Add item to cart
        /// - DELETE /api/v1/cart/items/{product_id}/{size} - Remove item from cart
        /// </remarks>
        public static void MapCartEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/cart")
                .WithTags("Cart")
                .RequireAuthorization();

            // Add item to cart
            group.MapPost("/items", AddCartItemHandler)
                .WithName("AddCartItem")
                .WithSummary("Add item to shopping cart")
                .WithDescription("Adds a new item to the cart or increments quantity if item already exists")
                .Produces(200)
                .Produces(400)
                .Produces(401)
                .Produces(404);

            // Remove item from cart
            group.MapDelete("/items/{product_id}/{size}", RemoveCartItemHandler)
                .WithName("RemoveCartItem")
                .WithSummary("Remove item from shopping cart")
                .WithDescription("Removes a specific item from the cart by product ID and size")
                .Produces(200)
                .Produces(400)
                .Produces(401)
                .Produces(404);
        }

        /// <summary>
        /// Handles adding an item to the cart.
        /// Critical fix #4: ShopperId extracted from JWT, not from request body.
        /// </summary>
        private static async Task<IResult> AddCartItemHandler(
            AddCartItemRequest request,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct)
        {
            var shopperId = user.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(shopperId))
                return Results.Unauthorized();

            try
            {
                var command = new AddCartItemCommand(
                    shopperId,
                    request.ProductId,
                    request.Size,
                    request.Qty);

                var result = await sender.Send(command, ct);
                return Results.Ok(result);
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
        /// Handles removing an item from the cart.
        /// Critical fix #4: ShopperId extracted from JWT, not from header.
        /// </summary>
        private static async Task<IResult> RemoveCartItemHandler(
            string product_id,
            string size,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct)
        {
            var shopperId = user.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(shopperId))
                return Results.Unauthorized();

            var invalid =
                EndpointHelpers.Require(product_id, "product_id") ??
                EndpointHelpers.Require(size, "size");
            if (invalid is not null)
                return invalid;

            try
            {
                var result = await sender.Send(
                    new RemoveCartItemCommand(shopperId, product_id, size),
                    ct);
                return Results.Ok(result);
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
    }
}
