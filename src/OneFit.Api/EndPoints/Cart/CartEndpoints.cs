using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Features.Cart.Commands.AddCartItem;
using OneFit.Application.Features.Cart.Commands.RemoveCartItem;

namespace OneFit.Api.EndPoints.Cart
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
                .WithTags("Cart");
           

            // Add item to cart
            group.MapPost("/items", AddCartItemHandler)
                .WithName("AddCartItem")
                .WithSummary("Add item to shopping cart")
                .WithDescription("Adds a new item to the cart or increments quantity if item already exists")
                .Produces(200)
                .Produces(400)
                .Produces(404);

            // Remove item from cart
            group.MapDelete("/items/{product_id}/{size}", RemoveCartItemHandler)
                .WithName("RemoveCartItem")
                .WithSummary("Remove item from shopping cart")
                .WithDescription("Removes a specific item from the cart by product ID and size")
                .Produces(200)
                .Produces(400)
                .Produces(404);
        }

        /// <summary>
        /// Handles adding an item to the cart
        /// </summary>
        private static async Task<IResult> AddCartItemHandler(
            AddCartItemRequest request,
            ISender sender,
            CancellationToken ct)
        {
            try
            {
                var command = new AddCartItemCommand(
                    request.ShopperId,
                    request.ProductId,
                    request.Size,
                    request.Qty);

                var result = await sender.Send(command, ct);
                return Results.Ok(result);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new
                {
                    error = new { code = ex.ErrorCode, message = ex.Message }
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

        /// <summary>
        /// Handles removing an item from the cart
        /// </summary>
        private static async Task<IResult> RemoveCartItemHandler(
            string product_id,
            string size,
            [FromHeader(Name = "X-Shopper-Id")] string shopperId,
            ISender sender,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(shopperId))
            {
                return Results.BadRequest(new
                {
                    error = new { code = "INVALID_REQUEST", message = "X-Shopper-Id header is required." }
                });
            }

            if (string.IsNullOrWhiteSpace(product_id))
            {
                return Results.BadRequest(new
                {
                    error = new { code = "INVALID_REQUEST", message = "product_id is required." }
                });
            }

            if (string.IsNullOrWhiteSpace(size))
            {
                return Results.BadRequest(new
                {
                    error = new { code = "INVALID_REQUEST", message = "size is required." }
                });
            }

            try
            {
                var result = await sender.Send(
                    new RemoveCartItemCommand(shopperId, product_id, size),
                    ct);
                return Results.Ok(result);
            }
            catch (NotFoundException ex)
            {
                return Results.NotFound(new
                {
                    error = new { code = ex.ErrorCode, message = ex.Message }
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
}
