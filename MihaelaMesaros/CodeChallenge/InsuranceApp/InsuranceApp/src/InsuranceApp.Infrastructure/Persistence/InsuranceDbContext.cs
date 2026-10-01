using InsuranceApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence;

public sealed class InsuranceDbContext(DbContextOptions<InsuranceDbContext> options) : DbContext(options)
{
    public DbSet<Country> Countries => Set<Country>();

    public DbSet<County> Counties => Set<County>();

    public DbSet<City> Cities => Set<City>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<BuildingType> BuildingTypes => Set<BuildingType>();

    public DbSet<Policy> Policies { get; set; }
    public DbSet<Broker> Brokers { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<FeeConfig> FeeConfigs { get; set; }
    public DbSet<RiskFactorConfig> RiskFactorConfigs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
    }
}