using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Domain.Enums;

namespace InsuranceManagement.UnitTests.Helpers
{
    internal static class TestDataFactory
    {
        public static Country CreateCountry(
            string name = "Romania",
            Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name
        };

        public static County CreateCounty(
            string name = "Cluj",
            Guid? countryId = null,
            Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            CountryId = countryId ?? Guid.NewGuid()
        };

        public static City CreateCity(
            string name = "Cluj-Napoca",
            Guid? countyId = null,
            Guid? id = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Name = name,
            CountyId = countyId ?? Guid.NewGuid()
        };

        public static Client CreateClient(
            string name = "John Doe",
            string identificationNumber = "ID-123",
            Guid? id = null,
            ClientType type = ClientType.Individual,
            string email = "",
            string phoneNumber = "",
            string? primaryAddress = null) => new()
        {
            Id = id ?? Guid.NewGuid(),
            Type = type,
            Name = name,
            IdentificationNumber = identificationNumber,
            Email = email,
            PhoneNumber = phoneNumber,
            PrimaryAddress = primaryAddress
        };

        public static Building CreateBuilding() => new()
        {
            ClientId = Guid.NewGuid(),
            Street = "Main Street",
            StreetNumber = "10",
            CityId = Guid.NewGuid(),
            ConstructionYear = 2000,
            Type = BuildingType.Residential,
            NumberOfFloors = 2,
            SurfaceArea = 120,
            InsuredValue = 100000
        };

        public static CreateBuildingDto CreateBuildingRequest() => new()
        {
            ClientId = Guid.NewGuid(),
            Street = "Main Street",
            StreetNumber = "10",
            CityId = Guid.NewGuid(),
            ConstructionYear = 2000,
            Type = BuildingType.Residential,
            NumberOfFloors = 2,
            SurfaceArea = 120,
            InsuredValue = 100000
        };

        public static UpdateBuildingDto CreateUpdateBuildingRequest() => new()
        {
            Street = "Updated Street",
            StreetNumber = "20A",
            CityId = Guid.NewGuid(),
            ConstructionYear = 2001,
            Type = BuildingType.Office,
            NumberOfFloors = 3,
            SurfaceArea = 150,
            InsuredValue = 150000
        };
    }
}
