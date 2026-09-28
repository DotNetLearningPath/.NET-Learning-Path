using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public sealed class InsuranceDbContext : DbContext
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

    public InsuranceDbContext(DbContextOptions<InsuranceDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InsuranceDbContext).Assembly);
    }
}
