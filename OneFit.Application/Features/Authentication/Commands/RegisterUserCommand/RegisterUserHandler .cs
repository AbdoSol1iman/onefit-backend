using MediatR;
using OneFit.Application.Common.Interfaces;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Domain.Entities;

namespace OneFit.Application.Features.Authentication.Commands.RegisterUserCommand
{
    public class RegisterUserHandler : IRequestHandler<RegisterUserCommand>
    {
        private readonly IIdentityService _identityService;
        private readonly IApplicationDbContext _context;

        public RegisterUserHandler(
            IIdentityService identityService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _context = context;
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

            var shopper = new Shopper
            {
                ShopperId = result.UserId,
                Name = $"{request.FirstName} {request.LastName}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Shoppers.Add(shopper);

            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}