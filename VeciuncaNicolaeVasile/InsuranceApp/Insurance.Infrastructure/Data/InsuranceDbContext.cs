using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Data
{
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : base(options)
        {
        }

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Building> Buildings => Set<Building>();
        public DbSet<Country> Countries => Set<Country>();
        public DbSet<County> Counties => Set<County>();
        public DbSet<City> Cities => Set<City>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
        }
    }
}
