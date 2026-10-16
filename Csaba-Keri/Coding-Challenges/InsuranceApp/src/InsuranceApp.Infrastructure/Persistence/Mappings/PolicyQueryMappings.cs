using InsuranceApp.Application.Brokers.Results;
using InsuranceApp.Application.Buildings.Results;
using InsuranceApp.Application.Clients.Results;
using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Application.Policies.Results;
using InsuranceApp.Infrastructure.Persistence.Entities;

namespace InsuranceApp.Infrastructure.Persistence.Mappings;

internal static class PolicyQueryMappings
{
    public static PolicyListItemResult ToPolicyListItemResult(this PolicyEntity entity, string currencyCode)
    {
        return new(
            Id: entity.Id,
            PolicyNumber: entity.PolicyNumber,
            Status: entity.Status,
            ClientId: entity.ClientId,
            BuildingId: entity.BuildingId,
            BrokerId: entity.BrokerId,
            CurrencyId: entity.CurrencyId,
            CurrencyCode: currencyCode,
            StartDate: entity.StartDate,
            EndDate: entity.EndDate,
            BasePremium: entity.BasePremium,
            FinalPremium: entity.FinalPremium,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt
        );
    }

    public static PolicyDetailsResult ToPolicyDetailsResult(
        this PolicyEntity policyEntity,
        string currencyCode,
        ClientResult clientResult,
        BuildingDetailsResult buildingDetailsResult,
        BrokerResult brokerResult
    )
    {
        return new(
            Policy: policyEntity.ToPolicyResult(currencyCode),
            Client: clientResult,
            Building: buildingDetailsResult,
            Broker: brokerResult
        );
    }

    public static PolicyResult ToPolicyResult(this PolicyEntity entity, string currencyCode)
    {
        return new(
            Id: entity.Id,
            PolicyNumber: entity.PolicyNumber,
            Status: entity.Status,
            ClientId: entity.ClientId,
            BuildingId: entity.BuildingId,
            BrokerId: entity.BrokerId,
            CurrencyId: entity.CurrencyId,
            CurrencyCode: currencyCode,
            CurrencyExchangeRateToBase: entity.CurrencyExchangeRateToBase,
            StartDate: entity.StartDate,
            EndDate: entity.EndDate,
            BasePremium: entity.BasePremium,
            FinalPremium: entity.FinalPremium,
            CreatedAt: entity.CreatedAt,
            UpdatedAt: entity.UpdatedAt,

            AppliedAdjustments: [.. entity.AppliedAdjustments
                .OrderBy(adjustment => adjustment.Source)
                .ThenBy(adjustment => adjustment.ConfigurationId)
                .Select(adjustment => adjustment.ToDomain())]
        );
    }

    public static ClientResult ToClientResult(this ClientEntity entity)
    {
        return new(
            Id: entity.Id,
            Type: entity.Type,
            IdentificationNumber: entity.IdentificationNumber,
            Name: entity.Name,
            Email: entity.Email,
            Phone: entity.Phone,
            PrimaryAddress: entity.PrimaryAddress
        );
    }

    public static BuildingDetailsResult ToBuildingDetailsResult(
        this BuildingEntity buildingEntity,
        CityEntity cityEntity,
        CountyEntity countyEntity,
        CountryEntity countryEntity
    )
    {
        return new(
            Building: buildingEntity.ToBuildingResult(),
            Geography: new(
                City: cityEntity.ToCityResult(),
                County: countyEntity.ToCountyResult(),
                Country: countryEntity.ToCountryResult()
            )
        );
    }

    public static BuildingResult ToBuildingResult(this BuildingEntity entity)
    {
        return new(
            Id: entity.Id,
            ClientId: entity.ClientId,
            Type: entity.Type,
            Address: new(
                CityId: entity.CityId,
                Street: entity.Street,
                Number: entity.Number
            ),
            ConstructionYear: entity.ConstructionYear,
            NumberOfFloors: entity.NumberOfFloors,
            SurfaceArea: entity.SurfaceArea,
            InsuredValue: entity.InsuredValue
        );
    }

    public static CityResult ToCityResult(this CityEntity entity)
    {
        return new(
            Id: entity.Id,
            Name: entity.Name,
            CountyId: entity.CountyId
        );
    }

    public static CountyResult ToCountyResult(this CountyEntity entity)
    {
        return new(
            Id: entity.Id,
            Name: entity.Name,
            CountryId: entity.CountryId
        );
    }

    public static CountryResult ToCountryResult(this CountryEntity entity)
    {
        return new(
            Id: entity.Id,
            Name: entity.Name
        );
    }

    public static BrokerResult ToBrokerResult(this BrokerEntity entity)
    {
        return new(
            Id: entity.Id,
            Code: entity.Code,
            Name: entity.Name,
            Email: entity.Email,
            Phone: entity.Phone,
            Status: entity.Status
        );
    }
}
