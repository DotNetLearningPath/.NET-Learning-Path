using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Infrastructure.Persistence.Configurations;
using InsuranceManagement.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Persistence
{
    public class InsuranceDbContext : DbContext
    {
        public InsuranceDbContext(DbContextOptions options) : base(options)
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

            modelBuilder.ApplyConfiguration(new BuildingConfiguration());
            modelBuilder.ApplyConfiguration(new ClientConfiguration());

            GeographySeedData.Seed(modelBuilder);
        }
    }
}
