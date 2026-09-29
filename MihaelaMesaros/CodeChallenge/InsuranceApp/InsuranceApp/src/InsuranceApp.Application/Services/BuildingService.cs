using InsuranceApp.Application.Abstractions.Persistence;
using InsuranceApp.Application.Abstractions.Services;
using InsuranceApp.Application.Common;
using InsuranceApp.Application.DTOs.Building;
using InsuranceApp.Domain.Constants;
using InsuranceApp.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace InsuranceApp.Application.Services;

public sealed class BuildingService(
    IBuildingRepository buildingRepository, 
    IClientRepository clientRepository,
    IGeographyRepository geographyRepository,
    ILogger<BuildingService> logger
) : IBuildingService
{
    public async Task<Result<BuildingDto>> GetBuildingByIdAsync(int buildingId, CancellationToken cancellationToken)
    {
        if (buildingId <= 0)
        {
            return Result<BuildingDto>.Failure(BuildingErrors.InvalidBuildingId);
        }

        var building = await buildingRepository.GetBuildingByIdAsync(buildingId, cancellationToken);

        if (building is null)
        {
            return Result<BuildingDto>.Failure(BuildingErrors.NotFound(buildingId));
        }

        return Result<BuildingDto>.Success(MapBuildingToDto(building));
    }

    public async Task<Result<IReadOnlyList<BuildingDto>>> GetBuildingsByClientAsync(int clientId, CancellationToken cancellationToken)
    {
        if (clientId <= 0)
        {
            return Result<IReadOnlyList<BuildingDto>>.Failure(BuildingErrors.InvalidClientId);
        }

        var client = await clientRepository.GetClientByIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            return Result<IReadOnlyList<BuildingDto>>.Failure(ClientErrors.NotFound(clientId));
        }

        var buildings = await buildingRepository.GetBuildingsByClientAsync(clientId, cancellationToken);

        var buildingDtos = buildings.Select(MapBuildingToDto).ToList();

        return Result<IReadOnlyList<BuildingDto>>.Success(buildingDtos);
    }

    public async Task<Result<BuildingDto>> CreateBuildingForClientAsync(int clientId, CreateBuildingDto createBuildingDto, CancellationToken cancellationToken)
    {
        if (clientId <= 0)
        {
            return Result<BuildingDto>.Failure(BuildingErrors.InvalidClientId);
        }

        var client = await clientRepository.GetClientByIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            return Result<BuildingDto>.Failure(ClientErrors.NotFound(clientId));
        }

        createBuildingDto = createBuildingDto with
        {
            AddressStreet = createBuildingDto.AddressStreet?.Trim()!,
            AddressStreetNumber = createBuildingDto.AddressStreetNumber?.Trim()!,
            RiskIndicators = createBuildingDto.RiskIndicators?.Trim()
        };

        var validationBuildingDetails = ValidateBuildingDetails(createBuildingDto);

        if (validationBuildingDetails is not null)
        {
            return Result<BuildingDto>.Failure(validationBuildingDetails);
        }

        var validationBuildingCity = await ValidateCityAsync(createBuildingDto.CityId, cancellationToken);

        if (validationBuildingCity is not null)
        {
            return Result<BuildingDto>.Failure(validationBuildingCity);
        }

        var building = new Building
        {
            ClientId = clientId,
            CityId = createBuildingDto.CityId,
            AddressStreet = createBuildingDto.AddressStreet,
            AddressStreetNumber = createBuildingDto.AddressStreetNumber,
            ConstructionYear = createBuildingDto.ConstructionYear,
            BuildingType = createBuildingDto.BuildingType,
            NumberOfFloors = createBuildingDto.NumberOfFloors,
            SurfaceArea = createBuildingDto.SurfaceArea,
            InsuredValue = createBuildingDto.InsuredValue,
            RiskIndicators = createBuildingDto.RiskIndicators,
            CreatedAt = DateTime.UtcNow
        };

        await buildingRepository.AddBuildingAsync(building, cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Building {BuildingId} created.", building.BuildingId);
        }

        return Result<BuildingDto>.Success(MapBuildingToDto(building));
    }

    public async Task<Result<BuildingDto>> UpdateBuildingAsync(int buildingId, UpdateBuildingDto updateBuildingDto, CancellationToken cancellationToken)
    {
        if (buildingId <= 0)
        {
            return Result<BuildingDto>.Failure(BuildingErrors.InvalidBuildingId);
        }

        updateBuildingDto = updateBuildingDto with
        {
            AddressStreet = updateBuildingDto.AddressStreet?.Trim()!,
            AddressStreetNumber = updateBuildingDto.AddressStreetNumber?.Trim()!,
            RiskIndicators = updateBuildingDto.RiskIndicators?.Trim()
        };

        var validationBuildingDetails = ValidateBuildingDetails(updateBuildingDto);

        if (validationBuildingDetails is not null)
        {
            return Result<BuildingDto>.Failure(validationBuildingDetails);
        }

        var validationBuildingCity = await ValidateCityAsync(updateBuildingDto.CityId, cancellationToken);

        if (validationBuildingCity is not null)
        {
            return Result<BuildingDto>.Failure(validationBuildingCity);
        }


        var building = await buildingRepository.GetBuildingForUpdateAsync(buildingId, cancellationToken);

        if (building is null)
        {
            return Result<BuildingDto>.Failure(BuildingErrors.NotFound(buildingId));
        }

        building.CityId = updateBuildingDto.CityId;
        building.AddressStreet = updateBuildingDto.AddressStreet;
        building.AddressStreetNumber = updateBuildingDto.AddressStreetNumber;
        building.ConstructionYear = updateBuildingDto.ConstructionYear;
        building.BuildingType = updateBuildingDto.BuildingType;
        building.NumberOfFloors = updateBuildingDto.NumberOfFloors;
        building.SurfaceArea = updateBuildingDto.SurfaceArea;
        building.InsuredValue = updateBuildingDto.InsuredValue;
        building.RiskIndicators = updateBuildingDto.RiskIndicators;
        building.ModifiedAt = DateTime.UtcNow;

        await buildingRepository.SaveBuildingChangesAsync(cancellationToken);

        if (logger.IsEnabled(LogLevel.Information))
        {
            logger.LogInformation("Building {BuildingId} updated.", buildingId);
        }

        return Result<BuildingDto>.Success(MapBuildingToDto(building));
    }

    private static BuildingDto MapBuildingToDto(Building building)
    {
        return new BuildingDto(
            building.BuildingId,
            building.ClientId,
            building.AddressStreet,
            building.AddressStreetNumber,
            building.CityId,
            building.ConstructionYear,
            building.BuildingType,
            building.NumberOfFloors,
            building.SurfaceArea,
            building.InsuredValue,
            building.RiskIndicators);
    }

    private async Task<Error?> ValidateCityAsync(int cityId, CancellationToken cancellationToken)
    {
        if (cityId <= 0)
        {
            return BuildingErrors.InvalidCityId;
        }

        var cityExists = await geographyRepository.CityExistsAsync(cityId, cancellationToken);

        return cityExists ? null : BuildingErrors.CityNotFound(cityId);
    }

    private static Error? ValidateBuildingDetails(IBuildingDetailsDto building)
    {
        if (string.IsNullOrWhiteSpace(building.AddressStreet))
        {
            return BuildingErrors.AddressStreetRequired;
        }

        if (building.AddressStreet.Length > BuildingConstraints.AddressStreetMaxLength)
        {
            return BuildingErrors.InvalidAddressStreetLength;
        }

        if (string.IsNullOrWhiteSpace(building.AddressStreetNumber))
        {
            return BuildingErrors.AddressStreetNumberRequired;
        }

        if (building.AddressStreetNumber.Length > BuildingConstraints.AddressStreetNumberMaxLength)
        {
            return BuildingErrors.InvalidAddressStreetNumberLength;
        }

        if (building.ConstructionYear < BuildingConstraints.MinConstructionYear 
            || building.ConstructionYear > DateTime.UtcNow.Year)
        {
            return BuildingErrors.InvalidConstructionYear;
        }

        if (!Enum.IsDefined(building.BuildingType))
        {
            return BuildingErrors.InvalidBuildingType;
        }

        if (building.NumberOfFloors < BuildingConstraints.MinNumberOfFloors 
            || building.NumberOfFloors > BuildingConstraints.MaxNumberOfFloors)
        {
            return BuildingErrors.InvalidNumberOfFloors;
        }

        var decimalValidationError = ValidateBuildingDecimalValues(building);

        if (decimalValidationError is not null)
        {
            return decimalValidationError;
        }

        if (!string.IsNullOrWhiteSpace(building.RiskIndicators) 
            && building.RiskIndicators.Length > BuildingConstraints.RiskIndicatorsMaxLength)
        {
            return BuildingErrors.InvalidRiskIndicatorsLength;
        }

        return null;
    }

    private static Error? ValidateBuildingDecimalValues(IBuildingDetailsDto building)
    {
        if (!DecimalValidation.HasValidScale(building.SurfaceArea, CommonConstraints.DecimalScale))
        {
            return BuildingErrors.InvalidSurfaceAreaScale;
        }

        if (building.SurfaceArea < BuildingConstraints.MinSurfaceArea 
            || building.SurfaceArea > BuildingConstraints.MaxSurfaceArea)
        {
            return BuildingErrors.InvalidSurfaceArea;
        }

        if (!DecimalValidation.HasValidScale(building.InsuredValue, CommonConstraints.DecimalScale))
        {
            return BuildingErrors.InvalidInsuredValueScale;
        }

        if (building.InsuredValue < BuildingConstraints.MinInsuredValue 
            || building.InsuredValue > BuildingConstraints.MaxInsuredValue)
        {
            return BuildingErrors.InvalidInsuredValue;
        }

        return null;
    }

}