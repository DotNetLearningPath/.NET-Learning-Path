using InsuranceApp.Infrastructure.Identity;
using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence;

public class InsuranceDbContext(
    DbContextOptions<InsuranceDbContext> options
) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public DbSet<ClientEntity> Clients => Set<ClientEntity>();
    public DbSet<BuildingEntity> Buildings => Set<BuildingEntity>();
    public DbSet<CountryEntity> Countries => Set<CountryEntity>();
    public DbSet<CountyEntity> Counties => Set<CountyEntity>();
    public DbSet<CityEntity> Cities => Set<CityEntity>();
    public DbSet<BrokerEntity> Brokers => Set<BrokerEntity>();
    public DbSet<CurrencyEntity> Currencies => Set<CurrencyEntity>();
    public DbSet<FeeConfigurationEntity> FeeConfigurations => Set<FeeConfigurationEntity>();
    public DbSet<RiskFactorConfigurationEntity> RiskFactorConfigurations => Set<RiskFactorConfigurationEntity>();
    public DbSet<PolicyEntity> Policies => Set<PolicyEntity>();
    public DbSet<PolicyAdjustmentEntity> PolicyAdjustments => Set<PolicyAdjustmentEntity>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(InsuranceDbContext).Assembly);
        GeographySeed.Configure(builder);
    }
}
