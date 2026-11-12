using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Domain.Entities.Metadata;
using InsuranceManagement.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Persistence
{
    public class InsuranceDbContext(
        DbContextOptions<InsuranceDbContext> options)
        : DbContext(options)
    {
        public DbSet<Admin> Admins => Set<Admin>();
        public DbSet<Broker> Brokers => Set<Broker>();
        public DbSet<Policy> Policies => Set<Policy>();
        public DbSet<PolicyCalculationSnapshot> PolicyCalculationSnapshots => Set<PolicyCalculationSnapshot>();

        public DbSet<Client> Clients => Set<Client>();
        public DbSet<Building> Buildings => Set<Building>();

        public DbSet<Country> Countries => Set<Country>();
        public DbSet<County> Counties => Set<County>();
        public DbSet<City> Cities => Set<City>();

        public DbSet<Currency> Currencies => Set<Currency>();
        public DbSet<FeeConfiguration> FeeConfigurations => Set<FeeConfiguration>();
        public DbSet<RiskFactorConfiguration> RiskFactorConfigurations => Set<RiskFactorConfiguration>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(InsuranceDbContext).Assembly);

            GeographySeedData.Seed(modelBuilder);
            CurrencySeedData.Seed(modelBuilder);
            FeeConfigurationSeedData.Seed(modelBuilder);
            RiskFactorConfigurationSeedData.Seed(modelBuilder);
            AdminBrokerSeedData.Seed(modelBuilder);
        }
    }
}
