using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Features.Cart.Commands.AddCartItem;
using OneFit.Application.Features.Cart.Commands.RemoveCartItem;

namespace OneFit.Api.EndPoints.Cart
{
    public static class CartEndpoints
    {
       
        public static void MapCartEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/cart");

            group.MapPost("/items", async (AddCartItemRequest request, ISender sender, CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(request.ShopperId) ||
                    string.IsNullOrWhiteSpace(request.ProductId) ||
                    string.IsNullOrWhiteSpace(request.Size) ||
                    request.Qty <= 0)
                {
                    return Results.BadRequest(new
                    {
                        error = new { code = "INVALID_REQUEST", message = "shopperId, productId, size and a positive qty are all required." }
                    });
                }

                try
                {
                    var command = new AddCartItemCommand(request.ShopperId, request.ProductId, request.Size, request.Qty);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(result);
                }
                catch (NotFoundException ex)
                {
                    return Results.BadRequest(new { error = new { code = ex.ErrorCode, message = ex.Message } });
                }
            })
            .WithName("AddCartItem");

          
            group.MapDelete("/items/{product_id}", async (
                string product_id,
                [FromHeader(Name = "X-Shopper-Id")] string shopperId,
                ISender sender,
                CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(shopperId))
                {
                    return Results.BadRequest(new
                    {
                        error = new { code = "INVALID_REQUEST", message = "X-Shopper-Id header is required." }
                    });
                }

                try
                {
                    var result = await sender.Send(new RemoveCartItemCommand(shopperId, product_id), ct);
                    return Results.Ok(result);
                }
                catch (NotFoundException ex)
                {
                    return Results.BadRequest(new { error = new { code = ex.ErrorCode, message = ex.Message } });
                }
            })
            .WithName("RemoveCartItem");
        }
    }
}
