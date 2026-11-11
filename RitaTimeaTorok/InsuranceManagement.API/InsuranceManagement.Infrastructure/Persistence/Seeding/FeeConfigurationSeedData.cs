using InsuranceManagement.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Persistence.Seeding
{
    public static class FeeConfigurationSeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            var effectiveFrom = new DateOnly(2026, 1, 1);

            modelBuilder.Entity<FeeConfiguration>().HasData(
                new
                {
                    Id = Guid.Parse("99999999-9999-4999-8999-999999999991"),
                    Name = "Standard broker fee",
                    Type = "BrokerCommission",
                    Percentage = 5m,
                    EffectiveFrom = effectiveFrom,
                    EffectiveTo = (DateOnly?)null,
                    IsActive = true
                },
                new
                {
                    Id = Guid.Parse("99999999-9999-4999-8999-999999999992"),
                    Name = "Standard admin fee",
                    Type = "AdminFee",
                    Percentage = 2m,
                    EffectiveFrom = effectiveFrom,
                    EffectiveTo = (DateOnly?)null,
                    IsActive = true
                });
        }
    }
}
