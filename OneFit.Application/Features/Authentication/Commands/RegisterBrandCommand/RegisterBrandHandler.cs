using MediatR;
using OneFit.Application.Common.Interfaces;
using OneFit.Domain.Entities;
using OneFit.Domain.Entities.Brands;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Authentication.Commands.RegisterBrandCommand
{
    public class RegisterBrandHandler
       : IRequestHandler<RegisterBrandCommand>
    {
        private readonly IIdentityService _identityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IApplicationDbContext _context;

        public RegisterBrandHandler(
            IIdentityService identityService,
            IFileStorageService fileStorageService,
            IApplicationDbContext context)
        {
            _identityService = identityService;
            _fileStorageService = fileStorageService;
            _context = context;
        }

        public async Task Handle(
            RegisterBrandCommand request,
            CancellationToken cancellationToken)
        {
            // 1. Create Identity User
            var result = await _identityService.CreateBrandAsync(
                request.FirstName,
                request.LastName,
                request.Email,
                request.Password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join(", ", result.Errors));
            }

            // Critical fix #12: Wrap the remaining steps in a try/catch
            // so that if anything fails after user creation, we compensate
            // by deleting the dangling Identity user.
            try
            {
                // 2. Create Brand
                var brand = new Brand
                {
                    BrandId = Guid.NewGuid().ToString(),
                    Name = request.BrandName,
                    ApplicationUserId = result.UserId,
                    Status = BrandStatusEnum.PendingVerification,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Brands.Add(brand);

                // 3. Upload verification document
                using var stream =
                    request.VerificationDocument.OpenReadStream();

                var storageKey = await _fileStorageService.UploadAsync(
                    stream,
                    request.VerificationDocument.FileName,
                    request.VerificationDocument.ContentType,
                    "brands/verification-documents",
                    cancellationToken);

                // 4. Save document information
                var document = new BrandDocument
                {
                    DocumentId = Guid.NewGuid(),
                    BrandId = brand.BrandId,
                    FileName = request.VerificationDocument.FileName,
                    ContentType = request.VerificationDocument.ContentType,
                    FileSize = request.VerificationDocument.Length,
                    StorageKey = storageKey,
                    UploadedAt = DateTime.UtcNow
                };

                _context.BrandDocuments.Add(document);

                // 5. Save Brand + Document
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // Compensate: delete the Identity user that was already created
                // to avoid a dangling user without a Brand record.
                await _identityService.DeleteUserAsync(result.UserId);
                throw;
            }
        }
    }
}
