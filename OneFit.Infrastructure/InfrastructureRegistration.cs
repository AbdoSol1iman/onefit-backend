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
using System.Security.Claims;
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
     
// ==================== Identity ====================

services.AddIdentityCore<ApplicationUser>(options =>
{
    options.User.RequireUniqueEmail = true;

    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan =
        TimeSpan.FromMinutes(15);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<OneFitDbContext>()
.AddSignInManager();


            // ==================== Authentication / JWT ====================

            services.Configure<JwtSettings>(
                configuration.GetSection("Jwt"));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = configuration
                    .GetSection("Jwt")
                    .Get<JwtSettings>()!;

                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,

                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,

                        ValidateLifetime = true,

                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
            });


            // ==================== Authorization ====================

            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireClaim(
                        ClaimTypes.Role,
                        "Admin");
                });

                options.AddPolicy("User", policy =>
                    policy.RequireRole("User"));

                options.AddPolicy("Brand", policy =>
                    policy.RequireRole("Brand"));
            });



            // Identity Services
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<OneFitDbContext>()
            .AddDefaultTokenProviders();

            // Token Generators
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IRefreshTokenGenerator, RefreshTokenGenerator>();

            // Authentication Service
            services.AddScoped<IAuthService, AuthService>();

            // Repository Services
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IWishlistRepository, WishlistRepository>();
            services.AddScoped<ICartRepository, CartRepository>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IApplicationDbContext, OneFitDbContext>();


            services.AddScoped<IFileStorageService, CloudinaryFileStorageService>();

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

