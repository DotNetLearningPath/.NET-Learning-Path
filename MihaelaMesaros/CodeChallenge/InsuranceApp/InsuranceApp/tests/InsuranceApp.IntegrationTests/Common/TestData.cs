using InsuranceApp.Application.DTOs.Building;
using InsuranceApp.Application.DTOs.Client;
using InsuranceApp.Application.DTOs.FeeConfig;
using InsuranceApp.Application.DTOs.Policy;
using InsuranceApp.Application.DTOs.Reports;
using InsuranceApp.Application.DTOs.RiskFactorConfig;
using InsuranceApp.Domain.Entities;
using InsuranceApp.Domain.Enums;

namespace InsuranceApp.IntegrationTests.Common;

internal static class TestData
{
    public static readonly Guid NonExistingId = new("99999999-9999-9999-9999-999999999999");
    public const string BaseCurrencyCode = "RON";

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
            CountyId = BrasovCountyId,
            Name = "Brasov"
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
        Address = "Str. Progresului 32",
        CreatedAt = DateTime.UtcNow
    };

    public static readonly Client ClientForRead2 = new()
    {
        ClientId = Guid.NewGuid(),
        ClientType = ClientType.Company,
        Name = "DaroImob",
        IdentificationNumber = "434543534543",
        Email = "daroimob@test.com",
        Phone = "070711223344",
        Address = "Piata Sfatului 5",
        CreatedAt = DateTime.UtcNow
    };

    public static readonly List<Client> ClientsList =
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

    public static readonly List<Building> BuildingsList =
    [
        new Building
        {
            BuildingId = Guid.NewGuid(),
            ClientId = ClientsList[0].ClientId,
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
        },
        new Building
        {
            BuildingId = Guid.NewGuid(),
            ClientId = ClientsList[1].ClientId,
            CityId = Cities[1].CityId,
            BuildingTypeId = BuildingTypes[1].BuildingTypeId,
            AddressStreet = "Progresului",
            AddressStreetNumber = "33",
            ConstructionYear = 2010,
            NumberOfFloors = 2,
            SurfaceArea = 68.50m,
            InsuredValue = 90_000.00m,
            RiskIndicators = "",
            CreatedAt = DateTime.UtcNow
        },
        new Building
        {
            BuildingId = Guid.NewGuid(),
            ClientId = ClientsList[1].ClientId,
            CityId = Cities[1].CityId,
            BuildingTypeId = BuildingTypes[2].BuildingTypeId,
            AddressStreet = "Baladei",
            AddressStreetNumber = "10",
            ConstructionYear = 1950,
            NumberOfFloors = 1,
            SurfaceArea = 100m,
            InsuredValue = 100_000.00m,
            RiskIndicators = "",
            CreatedAt = DateTime.UtcNow
        }
    ];

    #endregion


    #region Currency
    public static readonly Currency CurrencyForCreate = new()
    {
        CurrencyId = Guid.NewGuid(),
        Code = "EUR",
        Name = "Euro",
        ExchangeRateToBase = 5.22m,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    public static readonly Currency CurrencyForUpdate = new()
    {
        CurrencyId = Guid.NewGuid(),
        Code = "EUR",
        Name = "Euro",
        ExchangeRateToBase = 5.22m,
        IsActive = true,
        CreatedAt = DateTime.UtcNow,
        ModifiedAt = DateTime.UtcNow
    };

    public static readonly List<Currency> CurrenciesList =
    [
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
    ];
    #endregion


    #region FeeConfig
    public static readonly CreateFeeConfigDto FeeConfigDtoForCreate = new(
        "Standard broker fee",
        FeeType.BrokerCommission,
        2.5000m,
        DateTime.UtcNow.Date.AddYears(-1),
        DateTime.UtcNow.Date.AddYears(1),
        true
    );

    public static readonly UpdateFeeConfigDto FeeConfigDtoForUpdate = new(
        "Standard broker fee",
        FeeType.BrokerCommission,
        2.5000m,
        DateTime.UtcNow.Date.AddYears(-1),
        DateTime.UtcNow.Date.AddYears(1),
        true
    );

    public static readonly List<FeeConfig> FeeConfigsList =
    [
        new FeeConfig
        {
            FeeConfigId = Guid.NewGuid(),
            Name = "Standard broker fee",
            FeeType = FeeType.BrokerCommission,
            Percentage = 2.5000m,
            EffectiveFrom = DateTime.UtcNow.Date.AddYears(-1),
            EffectiveTo = DateTime.UtcNow.Date.AddYears(1),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new FeeConfig
        {
            FeeConfigId = Guid.NewGuid(),
            Name = "Admin fee",
            FeeType = FeeType.AdminFee,
            Percentage = 1.0000m,
            EffectiveFrom = DateTime.UtcNow.Date.AddYears(-1),
            EffectiveTo = null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    ];
    #endregion


    #region RiskFactorConfig
    public static readonly CreateRiskFactorConfigDto RiskFactorConfigDtoForCreate = new(
        RiskFactorLevel.Country,
        Countries[0].CountryId,
        5.25m,
        true);

    public static readonly UpdateRiskFactorConfigDto RiskFactorConfigDtoForUpdate = new(
            RiskFactorLevel.Country,
            Countries[1].CountryId,
            -3.50m,
            false);

    public static readonly List<RiskFactorConfig> RiskFactorConfigsList =
    [
        new()
        {
            RiskFactorConfigId = Guid.NewGuid(),
            Level = RiskFactorLevel.Country,
            ReferenceId = Countries[0].CountryId,
            AdjustmentPercentage = 5.25m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        },
        new()
        {
            RiskFactorConfigId = Guid.NewGuid(),
            Level = RiskFactorLevel.Country,
            ReferenceId = Countries[1].CountryId,
            AdjustmentPercentage = -2.50m,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    ];
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
        IsActive = true,
        CreatedAt = DateTime.UtcNow
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

    public static readonly Broker BrokerInactive = new()
    {
        BrokerId = Guid.NewGuid(),
        BrokerCode = "BR103",
        Name = "Third Broker",
        Email = "third.broker@test.com",
        Phone = "0733333333",
        CommissionPercentage = 10.50m,
        IsActive = false,
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
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        }
    ];

    #endregion


    #region Policies
    public static List<Policy> PoliciesList =>
    [
        new Policy
        {
            PolicyId = Guid.NewGuid(),
            PolicyNumber = $"{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            ClientId = BuildingsList[0].ClientId, // First Client
            BuildingId = BuildingsList[0].BuildingId, // Residential, Cluj
            BrokerId = BrokersList[0].BrokerId, // First Broker
            CurrencyId = CurrenciesList[1].CurrencyId, // EUR
            Status = PolicyStatus.Draft,
            StartDate = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 10, 1, 0, 0, 0, DateTimeKind.Utc),
            BasePremium = 1000m,
            FinalPremium = 1050m, //TotalFinalPremiumInBaseCurrency = 1050 * 5.22 = 5481
            CreatedAt = DateTime.UtcNow
        },
        new Policy
        {
            PolicyId = Guid.NewGuid(),
            PolicyNumber = $"{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            ClientId = BuildingsList[0].ClientId, // First Client
            BuildingId = BuildingsList[0].BuildingId, // Residential, Cluj
            BrokerId = BrokersList[0].BrokerId, // First Broker
            CurrencyId = CurrenciesList[1].CurrencyId, // EUR
            Status = PolicyStatus.Draft,
            StartDate = new DateTime(2026, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 12, 1, 0, 0, 0, DateTimeKind.Utc),
            BasePremium = 500m,
            FinalPremium = 550m, //TotalFinalPremiumInBaseCurrency = 550 * 5.22 = 2871
            CreatedAt = DateTime.UtcNow
        },
        //TotalFinalPremium = 1050 + 550 = 1600 EUR = 8352 RON
        new Policy
        {
            PolicyId = Guid.NewGuid(),
            PolicyNumber = $"{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            ClientId = BuildingsList[1].ClientId, // Second Client
            BuildingId = BuildingsList[1].BuildingId, // Office, Brasov
            BrokerId = BrokersList[1].BrokerId, // Second Broker
            CurrencyId = CurrenciesList[0].CurrencyId, // RON
            Status = PolicyStatus.Active,
            StartDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2027, 11, 1, 0, 0, 0, DateTimeKind.Utc),
            BasePremium = 2000m,
            FinalPremium = 2200m,
            ActivationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        },
        new Policy
        {
            PolicyId = Guid.NewGuid(),
            PolicyNumber = $"{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            ClientId = BuildingsList[1].ClientId, // Second Client
            BuildingId = BuildingsList[2].BuildingId, // Industrial, Brasov
            BrokerId = BrokersList[1].BrokerId, // Second Broker
            CurrencyId = CurrenciesList[0].CurrencyId, // RON
            Status = PolicyStatus.Active,
            StartDate = new DateTime(2027, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            EndDate = new DateTime(2028, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            BasePremium = 1500m,
            FinalPremium = 1650m,
            ActivationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        }
        //TotalFinalPremium = 2200 + 1650 = 3850 RON = TotalFinalPremiumInBaseCurrency
    ];

    public static Policy CreatePolicyForRead()
    {
        return new Policy
        {
            PolicyId = Guid.NewGuid(),
            PolicyNumber = $"{DateTime.UtcNow.Year}-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}",
            ClientId = BuildingForRead.ClientId,
            BuildingId = BuildingForRead.BuildingId,
            BrokerId = BrokerForCreate.BrokerId,
            CurrencyId = CurrencyForCreate.CurrencyId,
            Status = PolicyStatus.Draft,
            StartDate = DateTime.UtcNow.Date.AddDays(1),
            EndDate = DateTime.UtcNow.Date.AddYears(1),
            BasePremium = 1000m,
            FinalPremium = 1050m,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CreatePolicyDto PolicyDtoForCreate => new(
        BuildingForRead.ClientId,
        BuildingForRead.BuildingId,
        BrokerForCreate.BrokerId,
        CurrencyForCreate.CurrencyId,
        new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc),
        new DateTime(2027, 10, 1, 0, 0, 0, DateTimeKind.Utc),
        1000m);

    public static CancelPolicyDto PolicyDtoForCancel => new("This policy is being cancelled because it is no longer needed.");

    #endregion

}
