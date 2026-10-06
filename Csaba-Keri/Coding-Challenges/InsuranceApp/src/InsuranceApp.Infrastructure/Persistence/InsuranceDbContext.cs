using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence;

public class InsuranceDbContext(
    DbContextOptions<InsuranceDbContext> options
) : DbContext(options)
{
    public DbSet<ClientEntity> Clients => Set<ClientEntity>();
    public DbSet<BuildingEntity> Buildings => Set<BuildingEntity>();
    public DbSet<CountryEntity> Countries => Set<CountryEntity>();
    public DbSet<CountyEntity> Counties => Set<CountyEntity>();
    public DbSet<CityEntity> Cities => Set<CityEntity>();
    public DbSet<BrokerEntity> Brokers => Set<BrokerEntity>();
    public DbSet<CurrencyEntity> Currencies => Set<CurrencyEntity>();
    public DbSet<FeeConfigurationEntity> FeeConfigurations => Set<FeeConfigurationEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
        GeographySeed.Configure(modelBuilder);
    }
}
