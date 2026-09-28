using InsuranceApp.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace InsuranceApp.Infrastructure.Persistence;

internal static class GeographySeed
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        var romania = new CountryEntity(
            id: Guid.Parse("11111111-1111-4111-8111-111111111111"),
            name: "Romania"
        );

        var hungary = new CountryEntity(
            id: Guid.Parse("11111111-1111-4111-8111-111111111112"),
            name: "Hungary"
        );

        var cluj = new CountyEntity(
            id: Guid.Parse("22222222-2222-4222-8222-222222222222"),
            name: "Cluj",
            countryId: romania.Id
        );

        var hajduBihar = new CountyEntity(
            id: Guid.Parse("22222222-2222-4222-8222-222222222223"),
            name: "Hajdú-Bihar",
            countryId: hungary.Id
        );

        var clujNapoca = new CityEntity(
            id: Guid.Parse("33333333-3333-4333-8333-333333333333"),
            name: "Cluj-Napoca",
            countyId: cluj.Id
        );

        var turda = new CityEntity(
            id: Guid.Parse("33333333-3333-4333-8333-333333333334"),
            name: "Turda",
            countyId: cluj.Id
        );

        var debrecen = new CityEntity(
            id: Guid.Parse("33333333-3333-4333-8333-333333333335"),
            name: "Debrecen",
            countyId: hajduBihar.Id
        );

        var hajduszoboszlo = new CityEntity(
            id: Guid.Parse("33333333-3333-4333-8333-333333333336"),
            name: "Hajdúszoboszló",
            countyId: hajduBihar.Id
        );

        modelBuilder.Entity<CountryEntity>().HasData(romania, hungary);
        modelBuilder.Entity<CountyEntity>().HasData(cluj, hajduBihar);
        modelBuilder.Entity<CityEntity>().HasData(clujNapoca, turda, debrecen, hajduszoboszlo);
    }
}
