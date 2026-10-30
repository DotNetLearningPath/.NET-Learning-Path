using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportAPI.Application.Abstractions;
using SupportAPI.Domain.Entities;
using SupportAPI.Infrastructure.Persistence;
using SupportAPI.Infrastructure.Repositories;

namespace SupportAPI.Infrastructure
{
    public static class InfrastructureDependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SupportDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IRequestsRepository, RequestsRepository>();
            services.AddScoped<IUserProvisioningRepository, UserProvisioningRepository>();
            
            return services;
        }

    }
}
