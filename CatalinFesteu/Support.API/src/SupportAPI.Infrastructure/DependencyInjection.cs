using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SupportAPI.Application.Abstractions;
using SupportAPI.Domain.Entities;
using SupportAPI.Infrastructure.Persistence;
using SupportAPI.Infrastructure.Repositories;

namespace SupportAPI.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SupportDbContext>(options => options.UseNpgsql(connectionString));

            services.AddScoped<IRequestsRepository, RequestsRepository>();
            
            return services;
        }

        public static IServiceCollection AddUserRegistration(this IServiceCollection services)
        {
            services.AddIdentityCore<User>(options =>
            {
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<SupportDbContext>();

            return services;
        }
    }
}
