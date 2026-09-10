using MediatR;
using OneFit.Application.Features.Checkout.Commands;
using System.Security.Claims;

namespace OneFit.Api.EndPoints.Checkouts
{
    public static class CheckoutEndPoint
    {
        public static void MapCheckoutEndPoint(
            this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/checkout", async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var shopperId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(shopperId))
                    return Results.Unauthorized();

                var result = await sender.Send(
                    new CheckoutCommand
                    {
                        ShopperId = shopperId
                    },
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("Checkout")
            .WithTags("Checkout");
        }
    }
}
