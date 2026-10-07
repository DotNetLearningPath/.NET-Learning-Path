using FluentValidation;
using InsuranceApp.Application.Brokers.Commands;
using InsuranceApp.Application.Brokers.Results;
using InsuranceApp.Application.Buildings.Commands;
using InsuranceApp.Application.Buildings.Results;
using InsuranceApp.Application.Clients.Commands;
using InsuranceApp.Application.Clients.Queries;
using InsuranceApp.Application.Clients.Results;
using InsuranceApp.Application.Common.Pagination;
using InsuranceApp.Application.Currencies.Commands;
using InsuranceApp.Application.Currencies.Results;
using InsuranceApp.Application.Fees.Commands;
using InsuranceApp.Application.Fees.Results;
using InsuranceApp.Application.Geography.Results;
using InsuranceApp.Application.RiskFactors.Commands;
using InsuranceApp.Application.RiskFactors.Results;
using InsuranceApp.Domain.Brokers;
using InsuranceApp.Domain.Buildings;
using InsuranceApp.Domain.Clients;
using InsuranceApp.Domain.Fees;
using InsuranceApp.Domain.RiskFactors;
using InsuranceApp.Domain.RiskFactors.Targets;
using InsuranceApp.WebApi.Models.Brokers;
using InsuranceApp.WebApi.Models.Buildings;
using InsuranceApp.WebApi.Models.Clients;
using InsuranceApp.WebApi.Models.Common;
using InsuranceApp.WebApi.Models.Currencies;
using InsuranceApp.WebApi.Models.Fees;
using InsuranceApp.WebApi.Models.Geography;
using InsuranceApp.WebApi.Models.RiskFactors;
using InsuranceApp.WebApi.Models.RiskFactors.Targets;

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

    public static FeeType ToDomain(this FeeTypeDto type)
    {
        return type switch
        {
            FeeTypeDto.BrokerCommission => FeeType.BrokerCommission,
            FeeTypeDto.RiskAdjustment => FeeType.RiskAdjustment,
            FeeTypeDto.AdminFee => FeeType.AdminFee,
            
            _ => throw new ValidationException("Fee type is invalid.")
        };
    }

    public static RiskFactorLevel ToDomain(this RiskFactorLevelDto level)
    {
        return level switch
        {
            RiskFactorLevelDto.Country => RiskFactorLevel.Country,
            RiskFactorLevelDto.County => RiskFactorLevel.County,
            RiskFactorLevelDto.City => RiskFactorLevel.City,
            RiskFactorLevelDto.BuildingType => RiskFactorLevel.BuildingType,
            
            _ => throw new ValidationException("Risk factor level is invalid.")
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

    public static FeeTypeDto ToDto(this FeeType type)
    {
        return type switch
        {
            FeeType.BrokerCommission => FeeTypeDto.BrokerCommission,
            FeeType.RiskAdjustment => FeeTypeDto.RiskAdjustment,
            FeeType.AdminFee => FeeTypeDto.AdminFee,
            
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
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

    public static CreateCurrencyCommand ToCommand(this CreateCurrencyRequest request)
    {
        return new(
            Code: request.Code,
            Name: request.Name,
            ExchangeRateToBase: request.ExchangeRateToBase!.Value,
            IsActive: request.IsActive!.Value
        );
    }

    public static UpdateCurrencyCommand ToCommand(this UpdateCurrencyRequest request, Guid currencyId)
    {
        return new(
            CurrencyId: currencyId,
            Name: request.Name,
            ExchangeRateToBase: request.ExchangeRateToBase!.Value,
            IsActive: request.IsActive!.Value
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

    public static CreateFeeCommand ToCreateCommand(this SaveFeeRequest request)
    {
        return new(
            Name: request.Name,
            Type: request.Type!.Value.ToDomain(),
            Percentage: request.Percentage!.Value,
            EffectiveFrom: request.EffectiveFrom!.Value,
            EffectiveTo: request.EffectiveTo,
            IsActive: request.IsActive!.Value
        );
    }

    public static CreateRiskFactorCommand ToCreateCommand(this SaveRiskFactorRequest request)
    {
        return new(
            Level: request.Level!.Value.ToDomain(),
            CountryId: request.CountryId,
            CountyId: request.CountyId,
            CityId: request.CityId,
            BuildingType: request.BuildingType?.ToDomain(),
            AdjustmentPercentage: request.AdjustmentPercentage!.Value,
            IsActive: request.IsActive!.Value
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

    public static UpdateFeeCommand ToUpdateCommand(this SaveFeeRequest request, Guid feeId)
    {
        return new(
            FeeId: feeId,
            Name: request.Name,
            Type: request.Type!.Value.ToDomain(),
            Percentage: request.Percentage!.Value,
            EffectiveFrom: request.EffectiveFrom!.Value,
            EffectiveTo: request.EffectiveTo,
            IsActive: request.IsActive!.Value
        );
    }

    public static UpdateRiskFactorCommand ToUpdateCommand(this SaveRiskFactorRequest request, Guid riskFactorId)
    {
        return new(
            RiskFactorId: riskFactorId,
            Level: request.Level!.Value.ToDomain(),
            CountryId: request.CountryId,
            CountyId: request.CountyId,
            CityId: request.CityId,
            BuildingType: request.BuildingType?.ToDomain(),
            AdjustmentPercentage: request.AdjustmentPercentage!.Value,
            IsActive: request.IsActive!.Value
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

    public static CurrencyResponse ToResponse(this CurrencyResult result)
    {
        return new(
            Id: result.Id,
            Code: result.Code,
            Name: result.Name,
            ExchangeRateToBase: result.ExchangeRateToBase,
            IsActive: result.IsActive
        );
    }

    public static FeeResponse ToResponse(this FeeResult result)
    {
        return new(
            Id: result.Id,
            Name: result.Name,
            Type: result.Type.ToDto(),
            Percentage: result.Percentage,
            EffectiveFrom: result.EffectiveFrom,
            EffectiveTo: result.EffectiveTo,
            IsActive: result.IsActive
        );
    }

    public static RiskFactorResponse ToResponse(this RiskFactorResult result)
    {
        return new(
            Id: result.Id,
            Target: result.Target.ToResponse(),
            AdjustmentPercentage: result.AdjustmentPercentage,
            IsActive: result.IsActive
        );
    }

    public static RiskTargetResponse ToResponse(this RiskTarget target)
    {
        return target switch
        {
            CountryTarget country =>
                new CountryTargetResponse(CountryId: country.CountryId),
            
            CountyTarget county =>
                new CountyTargetResponse(CountyId: county.CountyId),
            
            CityTarget city => 
                new CityTargetResponse(CityId: city.CityId),
            
            BuildingTypeTarget buildingType =>
                new BuildingTypeTargetResponse(BuildingType: buildingType.Type.ToDto()),

            _ => throw new InvalidOperationException(
                $"Unsupported risk target type: {target.GetType().Name}."
            )
        };
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
