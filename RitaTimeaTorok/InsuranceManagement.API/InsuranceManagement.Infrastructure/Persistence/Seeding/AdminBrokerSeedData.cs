using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Persistence.Seeding
{
    public static class AdminBrokerSeedData
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>().HasData(
                new
                {
                    Id = Guid.Parse("bbbbbbbb-bbbb-4bbb-8bbb-bbbbbbbbbbb1"),
                    Name = "Administrator",
                    Email = "admin@insurance.local",
                    Role = "Admin"
                });

            modelBuilder.Entity<Broker>().HasData(
                new
                {
                    Id = Guid.Parse("cccccccc-cccc-4ccc-8ccc-ccccccccccc1"),
                    BrokerCode = "BRK-001",
                    Name = "Broker One",
                    Email = "broker.one@insurance.local",
                    PhoneNumber = "+40 700 000 001",
                    Status = BrokerStatus.Active,
                    CommissionPercentage = (decimal?)null
                },
                new
                {
                    Id = Guid.Parse("cccccccc-cccc-4ccc-8ccc-ccccccccccc2"),
                    BrokerCode = "BRK-002",
                    Name = "Broker Two",
                    Email = "broker.two@insurance.local",
                    PhoneNumber = "+40 700 000 002",
                    Status = BrokerStatus.Active,
                    CommissionPercentage = (decimal?)null
                });
        }
    }
}
