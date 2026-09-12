using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneFit.Application.Common.Interfaces.IRepositories;
using OneFit.Application.Features.Cart.Commands.AddCartItem;
using OneFit.Application.Features.Products;
using OneFit.Application.Features.Stylist;
using OneFit.Infrastructure.Persistence;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Persistence.Repositories;
using OneFit.Infrastructure.Persistence.Services;

namespace OneFit.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OneFitDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IProductQueryService, ProductQueryService>();
        services.AddSingleton<IStylistSessionStore, InMemoryStylistSessionStore>();
        services.AddScoped<IStylistOrchestrator, StylistOrchestrator>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IWishlistRepository, WishlistRepository>();
        services.AddScoped<ICartRepository, CartRepository>();

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(AddCartItemCommand).Assembly));

        return services;
    }
}
