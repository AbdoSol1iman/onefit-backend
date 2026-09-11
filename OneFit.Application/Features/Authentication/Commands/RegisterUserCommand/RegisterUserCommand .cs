using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Authentication.Commands.RegisterUserCommand
{
    public class RegisterUserCommand : IRequest
    {
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string ConfirmPassword { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
    }
}
