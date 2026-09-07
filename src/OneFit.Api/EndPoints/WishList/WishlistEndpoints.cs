using MediatR;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Features.WishList.Commands.AddWishlistItem;
using OneFit.Application.Features.WishList.Commands.RemoveWishlistItem;
using OneFit.Application.Features.WishList.Queries.GetWishlist;

namespace OneFit.Api.EndPoints.WishList
{
    public static class WishlistEndpoints
    {
        public static void MapWishlistEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/wishlist");

            group.MapPost("/items", async (AddWishlistItemRequest request, ISender sender, CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(request.ShopperId) ||
                    string.IsNullOrWhiteSpace(request.ProductId) ||
                    string.IsNullOrWhiteSpace(request.Size))
                {
                    return Results.BadRequest(new
                    {
                        error = new { code = "INVALID_REQUEST", message = "shopperId, productId and size are all required." }
                    });
                }

                try
                {
                    var command = new AddWishlistItemCommand(request.ShopperId, request.ProductId, request.Size);
                    var result = await sender.Send(command, ct);
                    return Results.Ok(result);
                }
                catch (NotFoundException ex)
                {
                    return Results.BadRequest(new { error = new { code = ex.ErrorCode, message = ex.Message } });
                }
            })
            .WithName("AddWishlistItem");


            group.MapDelete("/items/{wishlist_item_id}", async (string wishlist_item_id, ISender sender, CancellationToken ct) =>
            {
                try
                {
                    await sender.Send(new RemoveWishlistItemCommand(wishlist_item_id), ct);
                    return Results.Ok(new { deleted = true });
                }
                catch (NotFoundException ex)
                {
                    return Results.BadRequest(new { error = new { code = ex.ErrorCode, message = ex.Message } });
                }
            })
            .WithName("RemoveWishlistItem");


            group.MapGet("", async ([Microsoft.AspNetCore.Mvc.FromQuery(Name = "shopper_id")] string shopperId,
                ISender sender, CancellationToken ct) =>
            {
                if (string.IsNullOrWhiteSpace(shopperId))
                {
                    return Results.BadRequest(new
                    {
                        error = new { code = "INVALID_REQUEST", message = "shopperId query parameter is required." }
                    });
                }

                var items = await sender.Send(new GetWishlistQuery(shopperId), ct);
                return Results.Ok(new { items });
            })
            .WithName("GetWishlist");
        }
    }
}
