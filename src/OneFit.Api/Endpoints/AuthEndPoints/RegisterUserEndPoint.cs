using MediatR;
using Npgsql;
using OneFit.Application.Features.Authentication.Commands.RegisterUserCommand;

namespace OneFit.Api.Endpoints.AuthEndPoints
{
    public static class RegisterUserEndPoint
    {
        public static void MapRegisterUserEndPoint(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/auth/register", async (
                RegisterUserCommand command,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                try
                {
                    await sender.Send(command, cancellationToken);

                    return Results.Ok(new
                    {
                        message = "Registration successful. You can now login."
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
                catch (Exception ex) when (
                    ex.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("duplicate", StringComparison.OrdinalIgnoreCase))
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
                            message = "Registration failed. Please try again later."
                        },
                        statusCode: StatusCodes.Status500InternalServerError);
                }
            })
            .WithName("RegisterUser")
            .WithTags("Authentication");
        }
    }
}
