using Microsoft.AspNetCore.Mvc;
using OneFit.Application.Common.Interfaces.Authentication;
using OneFit.Application.Features.Authentication.DTOs;

namespace OneFit.Api.Endpoints.AuthEndPoints
{
    public static class LoginEndPoint
    {
        public static IEndpointRouteBuilder MapLoginEndPoint(
            this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/auth");

            group.MapPost("/login", LoginHandler)
                .WithName("Login")
                .WithTags("Authentication")
                .WithDescription("Login with email and password")
                .RequireRateLimiting("auth");

            return app;
        }

        private static async Task<IResult> LoginHandler(
            LoginRequestDTO request,
            [FromServices] IAuthService authService)
        {
            try
            {
                var result = await authService.LoginAsync(request);

                return Results.Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Results.Json(
                    new
                    {
                        message = ex.Message
                    },
                    statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (Exception)
            {
                // Don't leak internal exception details to the client
                return Results.Json(
                    new
                    {
                        message = "An error occurred during login. Please try again."
                    },
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}