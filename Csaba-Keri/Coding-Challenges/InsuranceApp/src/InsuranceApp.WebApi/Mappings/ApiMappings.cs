using FluentValidation;
using InsuranceApp.Application.Brokers.Commands;
using InsuranceApp.Application.Brokers.Results;
using InsuranceApp.Application.Buildings.Commands;
using InsuranceApp.Application.Buildings.Results;
using InsuranceApp.Application.Clients.Commands;
using InsuranceApp.Application.Clients.Queries;
using InsuranceApp.Application.Clients.Results;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Domain.Brokers;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Clients;
using InsuranceApp.WebApi.Models.Brokers;
using InsuranceApp.WebApi.Models.Buildings;
using InsuranceApp.WebApi.Models.Clients;
using InsuranceApp.WebApi.Models.Common;
using InsuranceApp.WebApi.Models.Geography;

namespace InsuranceApp.WebApi.Mappings;

internal static class ApiMappings
{
    public static ClientType ToDomain(this ClientTypeDto type)
    {
        return type switch
        {
            ClientTypeDto.Individual => ClientType.Individual,
            ClientTypeDto.Company => ClientType.Company,

            _ => throw new ValidationException("Client type is invalid.")
        };
    }

    public static BuildingType ToDomain(this BuildingTypeDto type)
    {
        return type switch
        {
            BuildingTypeDto.Residential => BuildingType.Residential,
            BuildingTypeDto.Office => BuildingType.Office,
            BuildingTypeDto.Industrial => BuildingType.Industrial,

            _ => throw new ValidationException("Building type is invalid.")
        };
    }

    public static BrokerStatus ToDomain(this BrokerStatusDto status)
    {
        return status switch
        {
            BrokerStatusDto.Active => BrokerStatus.Active,
            BrokerStatusDto.Inactive => BrokerStatus.Inactive,

            _ => throw new ValidationException("Broker status is invalid.")
        };
    }

    public static ClientTypeDto ToDto(this ClientType type)
    {
        return type switch
        {
            ClientType.Individual => ClientTypeDto.Individual,
            ClientType.Company => ClientTypeDto.Company,

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static BuildingTypeDto ToDto(this BuildingType type)
    {
        return type switch
        {
            BuildingType.Residential => BuildingTypeDto.Residential,
            BuildingType.Office => BuildingTypeDto.Office,
            BuildingType.Industrial => BuildingTypeDto.Industrial,

            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }

    public static BrokerStatusDto ToDto(this BrokerStatus status)
    {
        return status switch
        {
            BrokerStatus.Active => BrokerStatusDto.Active,
            BrokerStatus.Inactive => BrokerStatusDto.Inactive,

            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    public static CreateClientCommand ToCommand(this CreateClientRequest request)
    {
        return new(
            Type: request.Type!.Value.ToDomain(),
            IdentificationNumber: request.IdentificationNumber,
            Name: request.Name,
            Email: request.Email,
            Phone: request.Phone,
            PrimaryAddress: request.PrimaryAddress
        );
    }

    public static UpdateClientCommand ToCommand(this UpdateClientRequest request, Guid clientId)
    {
        return new(
            ClientId: clientId,
            Name: request.Name,
            Email: request.Email,
            Phone: request.Phone,
            PrimaryAddress: request.PrimaryAddress
        );
    }

    public static CreateBuildingCommand ToCreateCommand(this SaveBuildingRequest request, Guid clientId)
    {
        return new(
            ClientId: clientId,
            Type: request.Type!.Value.ToDomain(),
            Address: request.Address!.ToCommand(),
            ConstructionYear: request.ConstructionYear!.Value,
            NumberOfFloors: request.NumberOfFloors!.Value,
            SurfaceArea: request.SurfaceArea!.Value,
            InsuredValue: request.InsuredValue!.Value
        );
    }

    public static UpdateBuildingCommand ToUpdateCommand(this SaveBuildingRequest request, Guid buildingId)
    {
        return new(
            BuildingId: buildingId,
            Type: request.Type!.Value.ToDomain(),
            Address: request.Address!.ToCommand(),
            ConstructionYear: request.ConstructionYear!.Value,
            NumberOfFloors: request.NumberOfFloors!.Value,
            SurfaceArea: request.SurfaceArea!.Value,
            InsuredValue: request.InsuredValue!.Value
        );
    }

    private static BuildingAddressCommand ToCommand(this BuildingAddressRequest request)
    {
        return new(
            CityId: request.CityId!.Value,
            Street: request.Street,
            Number: request.Number
        );
    }

    public static CreateBrokerCommand ToCommand(this CreateBrokerRequest request)
    {
        return new(
            Code: request.Code,
            Name: request.Name,
            Email: request.Email,
            Phone: request.Phone,
            Status: request.Status!.Value.ToDomain()
        );
    }

    public static UpdateBrokerCommand ToCommand(this UpdateBrokerRequest request, Guid brokerId)
    {
        return new(
            BrokerId: brokerId,
            Name: request.Name,
            Email: request.Email,
            Phone: request.Phone
        );
    }

    public static PageQuery ToQuery(this PageRequest request)
    {
        return new(
            PageNumber: request.PageNumber,
            PageSize: request.PageSize
        );
    }

    public static SearchClientsQuery ToQuery(this SearchClientsRequest request)
    {
        return new(
            Name: request.Name,
            Identifier: request.Identifier,
            PageNumber: request.PageNumber,
            PageSize: request.PageSize
        );
    }

    public static ClientResponse ToResponse(this ClientResult result)
    {
        return new(
            Id: result.Id,
            Type: result.Type.ToDto(),
            IdentificationNumber: result.IdentificationNumber,
            Name: result.Name,
            Email: result.Email,
            Phone: result.Phone,
            PrimaryAddress: result.PrimaryAddress
        );
    }

    public static BuildingResponse ToResponse(this BuildingResult result)
    {
        return new(
            Id: result.Id,
            ClientId: result.ClientId,
            Type: result.Type.ToDto(),
            Address: result.Address.ToResponse(),
            ConstructionYear: result.ConstructionYear,
            NumberOfFloors: result.NumberOfFloors,
            SurfaceArea: result.SurfaceArea,
            InsuredValue: result.InsuredValue
        );
    }

    public static BuildingDetailsResponse ToResponse(this BuildingDetailsResult result)
    {
        var building = result.Building;
        var geography = result.Geography;

        return new(
            Id: building.Id,
            ClientId: building.ClientId,
            Type: building.Type.ToDto(),
            Address: building.Address.ToResponse(),
            ConstructionYear: building.ConstructionYear,
            NumberOfFloors: building.NumberOfFloors,
            SurfaceArea: building.SurfaceArea,
            InsuredValue: building.InsuredValue,
            Geography: new(
                Country: new(geography.Country.Id, geography.Country.Name),
                County: new(geography.County.Id, geography.County.Name),
                City: new(geography.City.Id, geography.City.Name)
            )
        );
    }

    public static BuildingAddressResponse ToResponse(this BuildingAddressResult result)
    {
        return new(
            CityId: result.CityId,
            Street: result.Street,
            Number: result.Number
        );
    }

    public static CountryResponse ToResponse(this CountryResult result)
    {
        return new(
            Id: result.Id,
            Name: result.Name
        );
    }

    public static CountyResponse ToResponse(this CountyResult result)
    {
        return new(
            Id: result.Id,
            Name: result.Name,
            CountryId: result.CountryId
        );
    }

    public static CityResponse ToResponse(this CityResult result)
    {
        return new(
            Id: result.Id,
            Name: result.Name,
            CountyId: result.CountyId
        );
    }

    public static BrokerResponse ToResponse(this BrokerResult result)
    {
        return new(
            Id: result.Id,
            Code: result.Code,
            Name: result.Name,
            Email: result.Email,
            Phone: result.Phone,
            Status: result.Status.ToDto()
        );
    }

    public static PagedResponse<TResponse> ToResponse<TResult, TResponse>(
        this PagedResult<TResult> result,
        Func<TResult, TResponse> toResponse
    )
    {
        return new(
            items: [.. result.Items.Select(toResponse)],
            pageNumber: result.PageNumber,
            pageSize: result.PageSize,
            totalCount: result.TotalCount
        );
    }
}
