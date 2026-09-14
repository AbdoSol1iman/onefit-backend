using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneFit.Application.Features.Authentication.Commands.RegisterBrandCommand;

namespace OneFit.Api.Endpoints.AuthEndPoints
{
    public static class RegisterBrandEndPoint
    {
        public static void MapRegisterBrandEndPoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/register/brand", async (
                  [FromForm] RegisterBrandCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    await sender.Send(command, cancellationToken);

                    return Results.Ok(new
                    {
                        message = "Brand registration submitted successfully. Waiting for admin verification."
                    });
                }
                catch (InvalidOperationException ex)
                {
                    // Business logic errors (e.g. email already exists)
                    return Results.BadRequest(new
                    {
                        message = ex.Message
                    });
                }
                catch (Exception)
                {
                    // Don't leak internal exception details to the client
                    return Results.Problem(
                        statusCode: StatusCodes.Status500InternalServerError,
                        title: "Brand registration failed. Please try again.");
                }
            })
            .DisableAntiforgery()
            .RequireRateLimiting("auth")
            .WithName("RegisterBrand")
            .WithTags("Authentication");
        }
    }
}
