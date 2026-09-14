using MediatR;
using OneFit.Application.Features.Orders.Queries;
using System.Security.Claims;

namespace OneFit.Api.Endpoints.Orders
{
    public static class GetOrdersEndPoint
    {
        public static void MapGetOrdersEndPoint(
            this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/v1/orders", async (
                ClaimsPrincipal user,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var shopperId = user.FindFirstValue("UserId");

                if (string.IsNullOrEmpty(shopperId))
                    return Results.Unauthorized();

                var result = await sender.Send(
                    new GetOrdersQuery
                    {
                        ShopperId = shopperId
                    },
                    cancellationToken);

                return Results.Ok(result);
            })
            .RequireAuthorization()
            .WithName("GetOrders")
            .WithTags("Orders");
        }
    }
}
