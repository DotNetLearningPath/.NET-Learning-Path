using InsuranceApp.Domain.Brokers;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Clients;
using InsuranceApp.Domain.Currencies;
using InsuranceApp.Domain.Fees;
using InsuranceApp.Domain.Geography;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Domain.RiskFactors.Targets;
using InsuranceApp.Infrastructure.Persistence.Entities;

namespace InsuranceApp.Infrastructure.Persistence.Mappings;

internal static class PersistenceMappings
{
    public static Client ToDomain(this ClientEntity entity)
    {
        return new(
            id: entity.Id,
            type: entity.Type,
            identificationNumber: entity.IdentificationNumber,
            name: entity.Name,
            email: entity.Email,
            phone: entity.Phone,
            primaryAddress: entity.PrimaryAddress
        );
    }

    public static Building ToDomain(this BuildingEntity entity)
    {
        return new(
            id: entity.Id,
            clientId: entity.ClientId,
            type: entity.Type,
            address: new(
                cityId: entity.CityId,
                street: entity.Street,
                number: entity.Number
            ),
            constructionYear: entity.ConstructionYear,
            numberOfFloors: entity.NumberOfFloors,
            surfaceArea: entity.SurfaceArea,
            insuredValue: entity.InsuredValue
        );
    }

    public static Country ToDomain(this CountryEntity entity)
    {
        return new(
            id: entity.Id,
            name: entity.Name
        );
    }

    public static County ToDomain(this CountyEntity entity)
    {
        return new(
            id: entity.Id,
            name: entity.Name,
            countryId: entity.CountryId
        );
    }

    public static City ToDomain(this CityEntity entity)
    {
        return new(
            id: entity.Id,
            name: entity.Name,
            countyId: entity.CountyId
        );
    }

    public static Broker ToDomain(this BrokerEntity entity)
    {
        return new(
            id: entity.Id,
            code: entity.Code,
            name: entity.Name,
            email: entity.Email,
            phone: entity.Phone,
            status: entity.Status
        );
    }

    public static Currency ToDomain(this CurrencyEntity entity)
    {
        return new(
            id: entity.Id,
            code: entity.Code,
            name: entity.Name,
            exchangeRateToBase: entity.ExchangeRateToBase,
            isActive: entity.IsActive
        );
    }

    public static FeeConfiguration ToDomain(this FeeConfigurationEntity entity)
    {
        return new(
            id: entity.Id,
            name: entity.Name,
            type: entity.Type,
            percentage: entity.Percentage,
            effectiveFrom: entity.EffectiveFrom,
            effectiveTo: entity.EffectiveTo,
            isActive: entity.IsActive
        );
    }

    public static RiskFactorConfiguration ToDomain(this RiskFactorConfigurationEntity entity)
    {
        return new(
            id: entity.Id,
            target: entity.ToTarget(),
            adjustmentPercentage: entity.AdjustmentPercentage,
            isActive: entity.IsActive
        );
    }

    public static RiskTarget ToTarget(this RiskFactorConfigurationEntity entity)
    {
        return (entity.Level, entity.CountryId, entity.CountyId, entity.CityId, entity.BuildingType) switch
        {
            (RiskFactorLevel.Country, Guid id, null, null, null) => new CountryTarget(id),
            (RiskFactorLevel.County, null, Guid id, null, null) => new CountyTarget(id),
            (RiskFactorLevel.City, null, null, Guid id, null) => new CityTarget(id),
            (RiskFactorLevel.BuildingType, null, null, null, BuildingType type) => new BuildingTypeTarget(type),
            
            _ => throw new InvalidOperationException($"Risk factor {entity.Id} has an invalid stored target.")
        };
    }

    public static ClientEntity ToEntity(this Client client)
    {
        return new(
            id: client.Id,
            type: client.Type,
            identificationNumber: client.IdentificationNumber,
            name: client.Name,
            email: client.Email,
            phone: client.Phone,
            primaryAddress: client.PrimaryAddress
        );
    }

    public static BuildingEntity ToEntity(this Building building)
    {
        return new(
            id: building.Id,
            clientId: building.ClientId,
            type: building.Type,
            cityId: building.Address.CityId,
            street: building.Address.Street,
            number: building.Address.Number,
            constructionYear: building.ConstructionYear,
            numberOfFloors: building.NumberOfFloors,
            surfaceArea: building.SurfaceArea,
            insuredValue: building.InsuredValue
        );
    }

    public static BrokerEntity ToEntity(this Broker broker)
    {
        return new(
            id: broker.Id,
            code: broker.Code,
            name: broker.Name,
            email: broker.Email,
            phone: broker.Phone,
            status: broker.Status
        );
    }

    public static CurrencyEntity ToEntity(this Currency currency)
    {
        return new(
            id: currency.Id,
            code: currency.Code,
            name: currency.Name,
            exchangeRateToBase: currency.ExchangeRateToBase,
            isActive: currency.IsActive
        );
    }

    public static FeeConfigurationEntity ToEntity(this FeeConfiguration fee)
    {
        return new(
            id: fee.Id,
            name: fee.Name,
            type: fee.Type,
            percentage: fee.Percentage,
            effectiveFrom: fee.EffectiveFrom,
            effectiveTo: fee.EffectiveTo,
            isActive: fee.IsActive
        );
    }

    public static RiskFactorConfigurationEntity ToEntity(this RiskFactorConfiguration riskFactor)
    {
        return riskFactor.Target switch
        {
            CountryTarget target => CreateRiskFactorEntity(riskFactor, countryId: target.CountryId),
            CountyTarget target => CreateRiskFactorEntity(riskFactor, countyId: target.CountyId),
            CityTarget target => CreateRiskFactorEntity(riskFactor, cityId: target.CityId),
            BuildingTypeTarget target => CreateRiskFactorEntity(riskFactor, buildingType: target.Type),

            _ => throw new InvalidOperationException("Unsupported risk factor target.")
        };
    }

    private static RiskFactorConfigurationEntity CreateRiskFactorEntity(
        RiskFactorConfiguration riskFactor,
        Guid? countryId = null,
        Guid? countyId = null,
        Guid? cityId = null,
        BuildingType? buildingType = null
    )
    {
        return new(
            id: riskFactor.Id,
            level: riskFactor.Target.Level,
            countryId: countryId,
            countyId: countyId,
            cityId: cityId,
            buildingType: buildingType,
            adjustmentPercentage: riskFactor.AdjustmentPercentage,
            isActive: riskFactor.IsActive
        );
    }
}
