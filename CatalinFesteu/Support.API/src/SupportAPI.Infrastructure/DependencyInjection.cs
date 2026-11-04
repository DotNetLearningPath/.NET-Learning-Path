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
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);

            services.AddDbContext<SupportDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IUserRequestsRepository, UserRequestsRepository>();

            return services;
        }

    }
}
