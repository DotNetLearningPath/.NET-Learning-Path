using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SupportAPI.Domain.Entities;

namespace SupportAPI.Infrastructure.Persistence
{
    public class SupportDbContext(
        DbContextOptions<SupportDbContext>options) : DbContext(options)
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Request> Requests { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.ApplyConfigurationsFromAssembly(typeof(SupportDbContext).Assembly);

            builder.HasDefaultSchema("support-db");
        }
    }
}
