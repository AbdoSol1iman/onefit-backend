using MediatR;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
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
                catch (PostgresException ex) when (ex.SqlState == "23505")
                {
                    return Results.Conflict(new
                    {
                        message = "An account with this email already exists."
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
