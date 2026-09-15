using MediatR;
using OneFit.Api.Endpoints.Shared;
using OneFit.Application.Common.Exceptions;
using OneFit.Application.Features.Checkout.Commands;
using System.Security.Claims;

namespace OneFit.Api.Endpoints.Checkouts
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

                try
                {
                    var result = await sender.Send(
                        new CheckoutCommand
                        {
                            ShopperId = shopperId
                        },
                        cancellationToken);

                    return Results.Ok(result);
                }
                catch (NotFoundException ex)
                {
                    return EndpointHelpers.FromNotFoundException(ex);
                }
                catch (InvalidOperationException ex)
                {
                    return EndpointHelpers.BadRequest("CHECKOUT_FAILED", ex.Message);
                }
                catch (Exception)
                {
                    return EndpointHelpers.InternalError();
                }
            })
            .RequireAuthorization()
            .WithName("Checkout")
            .WithTags("Checkout");
        }
    }
}
