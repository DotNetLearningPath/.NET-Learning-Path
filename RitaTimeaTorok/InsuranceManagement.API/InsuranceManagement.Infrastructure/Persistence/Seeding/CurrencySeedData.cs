using InsuranceManagement.Domain.Entities.Metadata;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Persistence.Seeding
{
    public static class CurrencySeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Currency>().HasData(
                new
                {
                    Id = Guid.Parse("66666666-6666-4666-8666-666666666666"),
                    Code = "RON",
                    Name = "Romanian Leu",
                    ExchangeRateToBase = 1.00m,
                    IsActive = true
                },
                new
                {
                    Id = Guid.Parse("77777777-7777-4777-8777-777777777777"),
                    Code = "EUR",
                    Name = "Euro",
                    ExchangeRateToBase = 5.00m,
                    IsActive = true
                },
                new
                {
                    Id = Guid.Parse("88888888-8888-4888-8888-888888888888"),
                    Code = "USD",
                    Name = "US Dollar",
                    ExchangeRateToBase = 4.50m,
                    IsActive = true
                });
        }
    }
}
