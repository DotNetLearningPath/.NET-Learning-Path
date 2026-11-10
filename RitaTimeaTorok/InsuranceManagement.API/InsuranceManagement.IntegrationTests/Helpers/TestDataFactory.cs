using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.IntegrationTests.Helpers
{
    internal static class TestDataFactory
    {
        public const string MainStreet = "Main Street";
        public const string MainStreetNumber = "10";

        public static CreateCountryDto CreateCountryRequest(
            string name = "Testland") => new()
        {
            Name = name
        };

        public static CreateCountyDto CreateCountyRequest(
            Guid countryId,
            string name = "Test County") => new()
        {
            Name = name,
            CountryId = countryId
        };

        public static CreateCityDto CreateCityRequest(
            Guid countyId,
            string name = "Test City") => new()
        {
            Name = name,
            CountyId = countyId
        };

        public static CreateClientDto CreateClientRequest(
            string name = "Test Client",
            string? identifier = null,
            string email = "client@example.com") => new()
        {
            Type = ClientType.Individual,
            Name = name,
            IdentificationNumber = identifier ?? Guid.NewGuid().ToString("N")[..20],
            Email = email
        };

        public static CreateBuildingDto CreateBuildingRequest(
            Guid clientId,
            Guid cityId) => new()
        {
            ClientId = clientId,
            CityId = cityId,
            Street = MainStreet,
            StreetNumber = MainStreetNumber,
            ConstructionYear = 2000,
            Type = BuildingType.Residential,
            NumberOfFloors = 2,
            SurfaceArea = 120,
            InsuredValue = 100000
        };
    }
}
