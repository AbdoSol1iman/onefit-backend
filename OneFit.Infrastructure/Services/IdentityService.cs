using Microsoft.AspNetCore.Identity;
using OneFit.Application.Common.Interfaces;
using OneFit.Infrastructure.Constants;
using OneFit.Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Infrastructure.Services
{
    public class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<(bool Succeeded, string[] Errors, string UserId)>
            CreateUserAsync(
                string firstName,
                string lastName,
                string email,
                string password)
        {
            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return (
                    false,
                    new[] { "Email already exists." },
                    string.Empty);
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,

                // Normal users are active immediately
                IsActive = true,

                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(
                user,
                password);

            if (!result.Succeeded)
            {
                return (
                    false,
                    result.Errors
                        .Select(e => e.Description)
                        .ToArray(),
                    string.Empty);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                AppRoles.User);

            if (!roleResult.Succeeded)
            {
                return (
                    false,
                    roleResult.Errors
                        .Select(e => e.Description)
                        .ToArray(),
                    string.Empty);
            }

            return (
                true,
                Array.Empty<string>(),
                user.Id);
        }

        public async Task<(bool Succeeded, string[] Errors, string UserId)>
            CreateBrandAsync(
                string firstName,
                string lastName,
                string email,
                string password)
        {
            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return (
                    false,
                    new[] { "Email already exists." },
                    string.Empty);
            }

            var brandUser = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,

                // Brand must wait for admin verification
                IsActive = false,

                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(
                brandUser,
                password);

            if (!result.Succeeded)
            {
                return (
                    false,
                    result.Errors
                        .Select(e => e.Description)
                        .ToArray(),
                    string.Empty);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                brandUser,
                AppRoles.Brand);

            if (!roleResult.Succeeded)
            {
                return (
                    false,
                    roleResult.Errors
                        .Select(e => e.Description)
                        .ToArray(),
                    string.Empty);
            }

            return (
                true,
                Array.Empty<string>(),
                brandUser.Id);
        }

        public async Task ActivateUserAsync(string userId)
        {
            var user =
                await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new InvalidOperationException(
                    "User not found.");
            }

            user.IsActive = true;

            var result =
                await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                throw new Exception(
                    string.Join(
                        ", ",
                        result.Errors.Select(
                            e => e.Description)));
            }
        }
    }
}
