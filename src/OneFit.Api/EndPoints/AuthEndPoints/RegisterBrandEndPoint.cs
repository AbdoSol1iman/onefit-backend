using MediatR;
using Microsoft.AspNetCore.Mvc;
using OneFit.Application.Features.Authentication.Commands.RegisterBrandCommand;

namespace OneFit.Api.EndPoints.AuthEndPoints
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
                await sender.Send(command, cancellationToken);

                return Results.Ok(new
                {
                    message = "Brand registration submitted successfully. Waiting for admin verification."
                });
            })
            .DisableAntiforgery()
            .WithName("RegisterBrand")
            .WithTags("Authentication");
        }
    }
}
