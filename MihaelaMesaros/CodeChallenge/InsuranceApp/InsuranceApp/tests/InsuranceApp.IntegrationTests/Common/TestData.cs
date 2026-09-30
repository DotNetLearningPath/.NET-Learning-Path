using InsuranceApp.Application.DTOs.Building;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.Application.DTOs.FeeConfig;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.IntegrationTests.Common;

internal static class TestData
{
    public static readonly Guid NonExistingId = new("99999999-9999-9999-9999-999999999999");

    #region Geography
    private static readonly Guid RomaniaId = Guid.NewGuid();
    private static readonly Guid HungaryId = Guid.NewGuid();

    private static readonly Guid ClujCountyId = Guid.NewGuid();
    private static readonly Guid BrasovCountyId = Guid.NewGuid();

    public static readonly List<Country> Countries =
    [
        new()
        {
            CountryId = RomaniaId,
            Name = "Romania"
        },
        new()
        {
            CountryId = HungaryId,
            Name = "Hungary"
        }
    ];

    public static readonly List<County> Counties =
    [
        new()
        {
            CountyId = ClujCountyId,
            CountryId = RomaniaId,
            Name = "Cluj"
        },
        new()
        {
            CountyId = BrasovCountyId,
            CountryId = RomaniaId,
            Name = "Brasov"
        }
    ];

    public static readonly List<City> Cities =
    [
        new()
        {
            CityId = Guid.NewGuid(),
            CountyId = ClujCountyId,
            Name = "Cluj-Napoca"
        },
        new()
        {
            CityId = Guid.NewGuid(),
            CountyId = ClujCountyId,
            Name = "Turda"
        }
    ];
    #endregion


    #region Clients
    public static readonly CreateClientDto ClientDtoForCreate = new(
        ClientType.Individual,
        "John Doe",
        "1980101223344",
        "john@test.com",
        "0712345678",
        "Cluj-Napoca");

    public static readonly UpdateClientDto ClientDtoForUpdate = new(
        "John Updated",
        "john.updated@test.com",
        "0700123456",
        "Bucharest");

    public static readonly Client ClientForRead = new()
    {
        ClientId = Guid.NewGuid(),
        ClientType = ClientType.Individual,
        Name = "John Doe",
        IdentificationNumber = "1980101223344",
        Email = "john@test.com",
        Phone = "0712345678",
        Address = "Cluj-Napoca",
        CreatedAt = DateTime.UtcNow
    };

    public static readonly List<Client> ClientsForSearch =
    [
        new()
        {
            ClientId = Guid.NewGuid(),
            ClientType = ClientType.Individual,
            Name = "John Doe",
            IdentificationNumber = "1980101223344",
            Email = "john@test.com",
            CreatedAt = DateTime.UtcNow
        },
        new()
        {
            ClientId = Guid.NewGuid(),
            ClientType = ClientType.Individual,
            Name = "John Smith",
            IdentificationNumber = "1990202334455",
            Email = "john.smith@test.com",
            CreatedAt = DateTime.UtcNow
        },
        new()
        {
            ClientId = Guid.NewGuid(),
            ClientType = ClientType.Company,
            Name = "Demo Company",
            IdentificationNumber = "RO12345678",
            Email = "office@demo.test",
            CreatedAt = DateTime.UtcNow
        }
    ];
    #endregion


    #region Building Types
    public static readonly List<BuildingType> BuildingTypes =
    [
        new()
        {
            BuildingTypeId = Guid.NewGuid(),
            Name = "Residential"
        },
        new()
        {
            BuildingTypeId = Guid.NewGuid(),
            Name = "Office"
        },
        new()
        {
            BuildingTypeId = Guid.NewGuid(),
            Name = "Industrial"
        }
    ];
    #endregion


    #region Buildings
    public static readonly Client BuildingClient = new()
    {
        ClientId = Guid.NewGuid(),
        ClientType = ClientType.Individual,
        Name = "Building Test Client",
        IdentificationNumber = $"TEST-{Guid.NewGuid():N}",
        Email = "building.test@test.com",
        Phone = "0712345678",
        Address = "Cluj-Napoca",
        CreatedAt = DateTime.UtcNow
    };

    public static readonly Building BuildingForRead = new()
    {
        BuildingId = Guid.NewGuid(),
        ClientId = BuildingClient.ClientId,
        CityId = Cities[0].CityId,
        BuildingTypeId = BuildingTypes[0].BuildingTypeId,
        AddressStreet = "Memorandumului",
        AddressStreetNumber = "25A",
        ConstructionYear = 2015,
        NumberOfFloors = 4,
        SurfaceArea = 185.50m,
        InsuredValue = 750_000.00m,
        RiskIndicators = "Flood zone",
        CreatedAt = DateTime.UtcNow
    };

    public static readonly CreateBuildingDto BuildingDtoForCreate = new(
        BuildingTypes[0].BuildingTypeId,
        "Memorandumului",
        "25A",
        Cities[0].CityId,
        2015,
        4,
        185.50m,
        750_000.00m,
        "Flood zone");

    public static readonly UpdateBuildingDto BuildingDtoForUpdate = new(
        BuildingTypes[1].BuildingTypeId,
        "Republicii",
        "10B",
        Cities[0].CityId,
        2020,
        6,
        350.50m,
        1_250_000.00m,
        "Earthquake risk zone");
    #endregion


    #region Currency
    public static Currency CurrencyForCreate = new Currency
    {
        CurrencyId = Guid.NewGuid(),
        Code = "EUR",
        Name = "Euro",
        ExchangeRateToBase = 5.11m,
        IsActive = true
    };

    public static Currency CurrencyForUpdate = new Currency
    {
        CurrencyId = Guid.NewGuid(),
        Code = "EUR",
        Name = "Euro",
        ExchangeRateToBase = 5.22m,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        ModifiedAt = DateTime.UtcNow
    };

    public static List<Currency> CurrenciesList = new List<Currency>
    {
        new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Code = "RON",
            Name = "Romanian Leu",
            ExchangeRateToBase = 1.00m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new Currency
        {
            CurrencyId = Guid.NewGuid(),
            Code = "EUR",
            Name = "Euro",
            ExchangeRateToBase = 5.22m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    };
    #endregion


    #region FeeConfig
    public static CreateFeeConfigDto FeeConfigDtoForCreate = new CreateFeeConfigDto(
        "Standard broker fee",
        FeeType.BrokerCommission,
        2.5000m,
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
        true
    );

    public static UpdateFeeConfigDto FeeConfigDtoForUpdate = new UpdateFeeConfigDto(
        "Standard broker fee",
        FeeType.BrokerCommission,
        2.5000m,
        new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
        true
    );

    public static List<FeeConfig> FeeConfigsList = new List<FeeConfig>
    {
        new FeeConfig
        {
            FeeConfigId = Guid.NewGuid(),
            Name = "Standard broker fee",
            FeeType = FeeType.BrokerCommission,
            Percentage = 2.5000m,
            EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new FeeConfig
        {
            FeeConfigId = Guid.NewGuid(),
            Name = "Admin fee",
            FeeType = FeeType.AdminFee,
            Percentage = 1.0000m,
            EffectiveFrom = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EffectiveTo = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    };
    #endregion

    #region Brokers

    public static readonly Broker BrokerForCreate = new()
    {
        BrokerId = Guid.NewGuid(),
        BrokerCode = "BR001",
        Name = "John Broker",
        Email = "john.broker@test.com",
        Phone = "0712345678",
        CommissionPercentage = 5.25m,
        IsActive = true
    };

    public static readonly Broker BrokerForUpdate = new()
    {
        BrokerId = Guid.NewGuid(),
        BrokerCode = "BR002",
        Name = "Broker For Update",
        Email = "broker.update@test.com",
        Phone = "0722345678",
        CommissionPercentage = 4.50m,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public static readonly List<Broker> BrokersList =
    [
        new()
    {
        BrokerId = Guid.NewGuid(),
        BrokerCode = "BR101",
        Name = "First Broker",
        Email = "first.broker@test.com",
        Phone = "0711111111",
        CommissionPercentage = 5.25m,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    },
    new()
    {
        BrokerId = Guid.NewGuid(),
        BrokerCode = "BR102",
        Name = "Second Broker",
        Email = "second.broker@test.com",
        Phone = "0722222222",
        CommissionPercentage = 3.50m,
        IsActive = false,
        CreatedAt = DateTime.UtcNow
    }
    ];

    #endregion
}
