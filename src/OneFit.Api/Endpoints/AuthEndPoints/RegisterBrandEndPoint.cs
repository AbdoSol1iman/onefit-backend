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
                catch (FluentValidation.ValidationException ex)
                {
                    return Results.BadRequest(new
                    {
                        message = ex.Errors.Select(e => e.ErrorMessage).FirstOrDefault()
                            ?? "Validation failed."
                    });
                }
                catch (Exception)
                {
                    return Results.Json(
                        new
                        {
                            message = "Brand registration failed. Please try again later."
                        },
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .DisableAntiforgery()
            .WithName("RegisterBrand")
            .WithTags("Authentication");
        }
    }
}
