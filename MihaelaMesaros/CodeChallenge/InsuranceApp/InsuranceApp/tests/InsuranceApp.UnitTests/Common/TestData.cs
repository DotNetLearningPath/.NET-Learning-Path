using InsuranceApp.Application.DTOs.Broker;
using InsuranceApp.Application.DTOs.Building;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.Application.DTOs.Currency;
using InsuranceApp.Application.DTOs.FeeConfig;
using InsuranceApp.Application.DTOs.RiskFactorConfig;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.UnitTests.Common;

internal static class TestData
{
    public static readonly Guid NonExistingId = new("99999999-9999-9999-9999-999999999999");
    public const string EmailDomain = "@test.com";

    public static readonly Guid CountryId = Guid.NewGuid();
    public static readonly Guid CountyId = Guid.NewGuid();
    public static readonly Guid CityId = Guid.NewGuid();
    public static readonly Guid ClientId = Guid.NewGuid();
    public static readonly Guid BuildingTypeId = Guid.NewGuid();
    public static readonly Guid CurrencyId = Guid.NewGuid();
    public static readonly Guid FeeConfigId = Guid.NewGuid();
    public static readonly Guid RiskFactorConfigId = Guid.NewGuid();
    public static readonly Guid RiskFactorReferenceId = Guid.NewGuid();
    public static readonly Guid RiskFactorUpdateReferenceId = Guid.NewGuid();

    public const decimal InvalidSurfaceAreaScale = 123.456m;
    public const decimal InvalidInsuredValueScale = 1000.999m;

    public const decimal InvalidExchangeRateScale = 4.12345m;

    public const decimal InvalidFeePercentageScale = 2.123m;

    public static readonly DateTime FeeEffectiveFrom = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static readonly DateTime FeeEffectiveTo = new(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);

    public static readonly decimal InvalidCommissionPercentageScale = 5.123m;

    public const decimal InvalidRiskFactorPercentageScale = 5.123m;


    #region Geography
    public static Country CreateCountry1()
    {
        return new Country
        {
            CountryId = CountryId,
            Name = "Romania"
        };
    }

    public static Country CreateCountry2()
    {
        return new Country
        {
            CountryId = Guid.NewGuid(),
            Name = "Hungary"
        };
    }

    public static County CreateCounty1()
    {
        return new County
        {
            CountyId = CountyId,
            CountryId = CountryId,
            Name = "Cluj"
        };
    }

    public static County CreateCounty2()
    {
        return new County
        {
            CountyId = Guid.NewGuid(),
            CountryId = CountryId,
            Name = "Brasov"
        };
    }

    public static City CreateCity1()
    {
        return new City
        {
            CityId = Guid.NewGuid(),
            CountyId = CountyId,
            Name = "Cluj-Napoca"
        };
    }

    public static City CreateCity2()
    {
        return new City
        {
            CityId = Guid.NewGuid(),
            CountyId = CountyId,
            Name = "Turda"
        };
    }

    #endregion


    #region Clients

    public static Client CreateClient1()
    {
        return new Client
        {
            ClientId = ClientId,
            ClientType = ClientType.Individual,
            Name = "John Doe",
            IdentificationNumber = "1980101223344",
            Email = "john@test.com",
            Phone = "0712345678",
            Address = "Cluj-Napoca",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Client CreateClient2()
    {
        return new Client
        {
            ClientId = Guid.NewGuid(),
            ClientType = ClientType.Individual,
            Name = "John Smith",
            IdentificationNumber = "2990101223344",
            Email = "john.smith@test.com",
            Phone = "0722345678",
            Address = "Bucharest",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateClientDto CreateClientDto()
    {
        return new CreateClientDto(
            ClientType.Individual,
            "John Doe",
            "1980101223344",
            "john@test.com",
            "0712345678",
            "Cluj-Napoca");
    }

    public static UpdateClientDto UpdateClientDto()
    {
        return new UpdateClientDto(
            "John Updated",
            "john.updated@test.com",
            "0700123456",
            "Bucharest");
    }

    #endregion


    #region Buildings
    public static Building CreateBuilding1()
    {
        return new Building
        {
            BuildingId = Guid.NewGuid(),
            ClientId = ClientId,
            CityId = CityId,
            BuildingTypeId = BuildingTypeId,
            AddressStreet = "Memorandumului",
            AddressStreetNumber = "25A",
            ConstructionYear = 2015,
            NumberOfFloors = 4,
            SurfaceArea = 185.50m,
            InsuredValue = 750_000.00m,
            RiskIndicators = "Flood zone",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Building CreateBuilding2()
    {
        return new Building
        {
            BuildingId = Guid.NewGuid(),
            ClientId = ClientId,
            CityId = CityId,
            BuildingTypeId = BuildingTypeId,
            AddressStreet = "Republicii",
            AddressStreetNumber = "10",
            ConstructionYear = 2020,
            NumberOfFloors = 6,
            SurfaceArea = 300.50m,
            InsuredValue = 1_000_000.00m,
            RiskIndicators = "Earthquake risk zone",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Client CreateClientForBuilding()
    {
        return new Client
        {
            ClientId = ClientId,
            ClientType = ClientType.Individual,
            Name = "John Doe",
            IdentificationNumber = "1980101223344",
            Email = "john@test.com",
            Phone = "0712345678",
            Address = "Cluj-Napoca",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateBuildingDto CreateBuildingDto()
    {
        return new CreateBuildingDto(
            BuildingTypeId,
            "Memorandumului",
            "25A",
            CityId,
            2015,
            4,
            185.50m,
            750_000.00m,
            "Flood zone");
    }

    public static UpdateBuildingDto UpdateBuildingDto()
    {
        return new UpdateBuildingDto(
            BuildingTypeId,
            "Republicii",
            "10",
            CityId,
            2020,
            6,
            300.50m,
            1_000_000.00m,
            "Earthquake risk zone");
    }

    #endregion


    #region Currencies

    public static Currency CreateCurrency1()
    {
        return new Currency
        {
            CurrencyId = CurrencyId,
            Code = "RON",
            Name = "Romanian Leu",
            ExchangeRateToBase = 1.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Currency CreateCurrency2()
    {
        return new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Code = "EUR",
            Name = "Euro",
            ExchangeRateToBase = 4.97m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateCurrencyDto CreateCurrencyDto()
    {
        return new CreateCurrencyDto(
            "RON",
            "Romanian Leu",
            1.00m,
            true);
    }

    public static UpdateCurrencyDto UpdateCurrencyDto()
    {
        return new UpdateCurrencyDto(
            "EUR",
            "Euro",
            4.97m,
            true);
    }

    #endregion


    #region Fee Configs

    public static FeeConfig CreateFeeConfig1()
    {
        return new FeeConfig
        {
            FeeConfigId = FeeConfigId,
            Name = "Standard broker fee",
            FeeType = FeeType.BrokerCommission,
            Percentage = 2.50m,
            EffectiveFrom = FeeEffectiveFrom,
            EffectiveTo = FeeEffectiveTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static FeeConfig CreateFeeConfig2()
    {
        return new FeeConfig
        {
            FeeConfigId = Guid.NewGuid(),
            Name = "Admin fee",
            FeeType = FeeType.AdminFee,
            Percentage = 1.50m,
            EffectiveFrom = FeeEffectiveFrom,
            EffectiveTo = FeeEffectiveTo,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateFeeConfigDto CreateFeeConfigDto()
    {
        return new CreateFeeConfigDto(
            "Standard broker fee",
            FeeType.BrokerCommission,
            2.50m,
            FeeEffectiveFrom,
            FeeEffectiveTo,
            true);
    }

    public static UpdateFeeConfigDto UpdateFeeConfigDto()
    {
        return new UpdateFeeConfigDto(
            "Updated broker fee",
            FeeType.BrokerCommission,
            5.50m,
            FeeEffectiveFrom,
            FeeEffectiveTo,
            false);
    }

    #endregion


    #region Risk Factor Configs

    public static RiskFactorConfig CreateRiskFactorConfig1()
    {
        return new RiskFactorConfig
        {
            RiskFactorConfigId = RiskFactorConfigId,
            Level = RiskFactorLevel.Country,
            ReferenceId = RiskFactorReferenceId,
            AdjustmentPercentage = 5.25m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static RiskFactorConfig CreateRiskFactorConfig2()
    {
        return new RiskFactorConfig
        {
            RiskFactorConfigId = Guid.NewGuid(),
            Level = RiskFactorLevel.City,
            ReferenceId = Guid.NewGuid(),
            AdjustmentPercentage = -2.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateRiskFactorConfigDto CreateRiskFactorConfigDto()
    {
        return new CreateRiskFactorConfigDto(
            RiskFactorLevel.Country,
            RiskFactorReferenceId,
            5.25m,
            true);
    }

    public static UpdateRiskFactorConfigDto UpdateRiskFactorConfigDto()
    {
        return new UpdateRiskFactorConfigDto(
            RiskFactorLevel.County,
            RiskFactorReferenceId,
            -2.50m,
            true);
    }

    #endregion


    #region Brokers
    public static Broker CreateBroker1()
    {
        return new Broker
        {
            BrokerId = Guid.NewGuid(),
            BrokerCode = "BR001",
            Name = "Test Broker 01",
            Email = $"broker01{EmailDomain}",
            Phone = "0712345678",
            CommissionPercentage = 5.25m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Broker CreateBroker2()
    {
        return new Broker
        {
            BrokerId = Guid.NewGuid(),
            BrokerCode = "BR002",
            Name = "Test Broker 02",
            Email = $"broker02{EmailDomain}",
            Phone = "0712345678",
            CommissionPercentage = 4.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreateBrokerDto CreateBrokerDto()
    {
        return new CreateBrokerDto(
            "BR001",
            "Test Broker",
            $"broker{EmailDomain}",
            "0712345678",
            5.25m,
            true);
    }

    public static UpdateBrokerDto UpdateBrokerDto()
    {
        return new UpdateBrokerDto(
            "BR001",
            "Test Broker Updated",
            $"updated.broker{EmailDomain}",
            "0722345678",
            3.50m);
    }
    #endregion
}
