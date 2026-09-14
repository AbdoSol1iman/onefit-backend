using MediatR;
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
                        title: "Registration failed. Please try again.");
                }
            })
            .RequireRateLimiting("auth")
            .WithName("RegisterUser")
            .WithTags("Authentication");
        }
    }
}
