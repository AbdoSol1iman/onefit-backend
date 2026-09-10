using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OneFit.Application.Common.Interfaces;
using OneFit.Application.Common.Interfaces.Authentication;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.Catalog.Queries.QueryCatalog;
using OneFit.Infrastructure.Authentication;
using OneFit.Infrastructure.FileStorage;
using OneFit.Infrastructure.Identity;
using OneFit.Infrastructure.Persistence;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Persistence.Repositories;
using OneFit.Infrastructure.Services;
using System;
using System.Text;

namespace OneFit.Infrastructure
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Database & DbContext
            services.AddDbContext<OneFitDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("DefaultConnection")));

            services.AddDataProtection();

            // Identity
            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddSignInManager()
                .AddEntityFrameworkStores<OneFitDbContext>()
                .AddDefaultTokenProviders();

            // JWT Settings
            services.Configure<JwtSettings>(
                configuration.GetSection("Jwt"));

            var jwtSettings = configuration
                .GetSection("Jwt")
                .Get<JwtSettings>()
                ?? throw new InvalidOperationException(
                    "JWT settings are not configured.");

            // Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,

                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,

                        IssuerSigningKey =
                            new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    jwtSettings.SecretKey))
                    };
            });

            // Application Services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IApplicationDbContext, OneFitDbContext>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();
            services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();

            // Cloudinary
            services.Configure<CloudinarySettings>(
                configuration.GetSection("CloudinarySettings"));

            // MediatR
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(
                    typeof(QueryCatalogQuery).Assembly));

            return services;
        }

        public static async Task SeedIdentityAsync(
            this IServiceProvider services)
        {
            await IdentitySeeder.SeedAsync(services);
        }
    }
}