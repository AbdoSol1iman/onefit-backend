using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Application.Features.Authentication.Commands.RegisterBrandCommand
{
    public class RegisterBrandValidator
     : AbstractValidator<RegisterBrandCommand>
    {
        private static readonly string[] AllowedExtensions =
        {
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf"
    };

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        public RegisterBrandValidator()
        {
            RuleFor(x => x.BrandName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.LastName)
                .NotEmpty()
                .MaximumLength(100);

            RuleFor(x => x.Email)
                .NotEmpty()
                .EmailAddress();

            RuleFor(x => x.Password)
                .NotEmpty()
                .MinimumLength(8);

            RuleFor(x => x.ConfirmPassword)
                .NotEmpty()
                .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");

            RuleFor(x => x.VerificationDocument)
                .NotNull()
                .Must(file => file.Length > 0)
                .WithMessage("Verification document is required.")
                .Must(file =>
                    AllowedExtensions.Contains(
                        Path.GetExtension(file.FileName).ToLowerInvariant()))
                .WithMessage("Only JPG, JPEG, PNG, and PDF files are allowed.")
                .Must(file => file.Length <= MaxFileSize)
                .WithMessage("File size must not exceed 5 MB.");
        }
    }
}
