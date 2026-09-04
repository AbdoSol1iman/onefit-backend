using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OneFit.Infrastructure.Persistence.Data;
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
            return services;
        }
    }
}