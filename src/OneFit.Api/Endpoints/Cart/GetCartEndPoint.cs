using MediatR;
using OneFit.Application.Features.Cart.Queries.GetCart;
using System.Security.Claims;

namespace OneFit.Api.Endpoints.Cart
{
    public static class GetCartEndPoint
    {
        public static void MapGetCartEndPoint(
            this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/cart", async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var shopperId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(shopperId))
                {
                    return Results.Unauthorized();
                }

                var result = await sender.Send(
                    new GetCartQuery
                    {
                        ShopperId = shopperId
                    },
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetCart")
            .WithTags("Cart");
        }
    }

}
