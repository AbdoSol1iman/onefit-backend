using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneFit.Application.Features.Catalog;
using OneFit.Infrastructure.Persistence.Data;
using OneFit.Infrastructure.Persistence.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace OneFit.Infrastructure
{
    public static class InfrastructureRegistration
    {
        public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OneFitDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<ICatalogQueryService, CatalogQueryService>();
            return services;
        }
    }
}