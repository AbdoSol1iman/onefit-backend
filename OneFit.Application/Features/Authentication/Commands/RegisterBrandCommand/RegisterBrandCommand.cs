using MediatR;
using Microsoft.AspNetCore.Http;

namespace OneFit.Application.Features.Authentication.Commands.RegisterBrandCommand
{
    public class RegisterBrandCommand : IRequest
    {
        public string BrandName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public IFormFile VerificationDocument { get; set; } = null!;
    }
}