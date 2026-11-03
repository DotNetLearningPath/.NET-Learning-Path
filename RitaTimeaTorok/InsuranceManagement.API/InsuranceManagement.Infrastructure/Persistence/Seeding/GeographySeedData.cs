using InsuranceManagement.Domain.Entities.Geography;
using Microsoft.EntityFrameworkCore;

namespace InsuranceManagement.Infrastructure.Persistence.Seeding;

public static class GeographySeedData
{
    public static void Seed(ModelBuilder modelBuilder)
    {
        var romaniaId = Guid.Parse(
            "11111111-1111-4111-8111-111111111111");

        var clujCountyId = Guid.Parse(
            "22222222-2222-4222-8222-222222222222");

        var bucharestCountyId = Guid.Parse(
            "33333333-3333-4333-8333-333333333333");

        modelBuilder.Entity<Country>().HasData(
            new
            {
                Id = romaniaId,
                Name = "Romania"
            });

        modelBuilder.Entity<County>().HasData(
            new
            {
                Id = clujCountyId,
                Name = "Cluj",
                CountryId = romaniaId
            },
            new
            {
                Id = bucharestCountyId,
                Name = "Bucuresti",
                CountryId = romaniaId
            });

        modelBuilder.Entity<City>().HasData(
            new
            {
                Id = Guid.Parse("44444444-4444-4444-8444-444444444444"),
                Name = "Cluj-Napoca",
                CountyId = clujCountyId
            },
            new
            {
                Id = Guid.Parse("55555555-5555-4555-8555-555555555555"),
                Name = "Bucuresti",
                CountyId = bucharestCountyId
            });
    }
}