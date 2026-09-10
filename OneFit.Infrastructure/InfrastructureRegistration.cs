using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OneFit.Application.Common.Interfaces;
using OneFit.Application.Common.Interfaces.Authentication;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.Catalog;
using OneFit.Application.Features.Catalog.Queries.QueryCatalog;
using OneFit.Infrastructure.Authentication;
using OneFit.Infrastructure.FileStorage;
using OneFit.Infrastructure.Identity;
using OneFit.Infrastructure.Persistence;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Persistence.Repositories;
using OneFit.Infrastructure.Services;
using System;
using System.Collections.Generic;
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
            services.AddAuthentication();
            services.AddDataProtection();

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole>()
                .AddSignInManager()
                .AddEntityFrameworkStores<OneFitDbContext>()
                .AddDefaultTokenProviders();

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