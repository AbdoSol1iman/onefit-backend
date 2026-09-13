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
                catch (Exception ex)
                {
                    return Results.BadRequest(new
                    {
                        message = ex.Message
                    });
                }
            })
            .WithName("RegisterUser")
            .WithTags("Authentication");
        }
    }
}
