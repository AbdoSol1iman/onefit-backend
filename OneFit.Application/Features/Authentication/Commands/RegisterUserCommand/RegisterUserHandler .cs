using MediatR;
using OneFit.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Authentication.Commands.RegisterUserCommand
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand>
    {
        private readonly IIdentityService _identityService;

        public RegisterUserHandler(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        public async Task Handle(
            RegisterUserCommand request,
            CancellationToken cancellationToken)
        {
            var result = await _identityService.CreateUserAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password);

            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors));
            }
        }
    }
}
