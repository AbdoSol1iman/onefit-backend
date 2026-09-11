using Microsoft.AspNetCore.Identity;
using OneFit.Application.Common.Interfaces.Authentication;
using OneFit.Application.Features.Authentication.DTOs;
using OneFit.Infrastructure.Identity;

namespace OneFit.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenGenerator _refreshTokenGenerator;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenGenerator jwtTokenGenerator,
        IRefreshTokenGenerator refreshTokenGenerator)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _refreshTokenGenerator = refreshTokenGenerator;
    }

    public async Task<LoginResponseDTO> LoginAsync(
        LoginRequestDTO request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Your account is not active. Please wait for admin verification.");
        }

        var result = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: false);

        if (!result.Succeeded)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var role = roles.FirstOrDefault() ?? string.Empty;

        var accessToken = _jwtTokenGenerator.GenerateToken(
            user.Id,
            user.Email!,
            role);

        var refreshToken = _refreshTokenGenerator.GenerateToken();

        return new LoginResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            User = new UserProfileDto
            {
                Id = user.Id,
                Email = user.Email!,
                Role = role
            }
        };
    }
}