using InsuranceApp.Domain.Brokers;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Clients;
using InsuranceApp.Domain.Geography;
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
}
