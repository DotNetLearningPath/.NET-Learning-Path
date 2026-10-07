using Insurance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Insurance.Infrastructure.Persistence;

public sealed class InsuranceDbContext(
    DbContextOptions<InsuranceDbContext> options) : DbContext(options)
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Building> Buildings => Set<Building>();
    public DbSet<Country> Countries => Set<Country>();
    public DbSet<County> Counties => Set<County>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Broker> Brokers => Set<Broker>();
    public DbSet<Currency> Currencies => Set<Currency>();
    public DbSet<FeeConfiguration> FeeConfigurations => Set<FeeConfiguration>();
    public DbSet<RiskFactorConfiguration> RiskFactorConfigurations => Set<RiskFactorConfiguration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InsuranceDbContext).Assembly);
    }
}
