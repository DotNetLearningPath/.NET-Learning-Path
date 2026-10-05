using Insurance.Application.Abstractions;
using Insurance.Application.DTO.Buildings;
using Insurance.Application.DTO.Common;
using Insurance.Application.Exceptions;
using Insurance.Domain.Entities;

namespace Insurance.Application.Services;

public class BuildingService(
        IBuildingRepository buildingRepository,
        IClientRepository clientRepository,
        IGeographyRepository geographyRepository) : IBuildingService
{
    private async Task CheckClientExistAsync(Guid clientId, CancellationToken cancellationToken)
    {
        if (await clientRepository.GetClientByIdAsync(clientId, cancellationToken) is null)
        {
            throw new NotFoundException("Client was not found.");
        }
    }

    private async Task CheckCityExistAsync(Guid cityId, CancellationToken cancellationToken)
    {
        var cityExists = await geographyRepository.CityExistsAsync(cityId, cancellationToken);

        if (!cityExists)
        {
            throw new NotFoundException("City was not found.");
        }
    }

    public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingRequest request, CancellationToken cancellationToken)
    {
        await CheckClientExistAsync(request.ClientId, cancellationToken);
        await CheckCityExistAsync(request.CityId, cancellationToken);

        var building = new Building(
            request.ClientId,
            request.CityId,
            request.Street,
            request.Number,
            request.ConstructionYear,
            request.Type,
            request.NumberOfFloors,
            request.SurfaceArea,
            request.InsuredValue,
            request.IsFloodRiskZone,
            request.IsEarthquakeRiskZone);

        await buildingRepository.AddBuildingAsync(building, cancellationToken);

        return MapToBuildingDto(building);
    }

    public async Task<BuildingDto?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var building = await buildingRepository.GetBuildingByIdAsync(id, cancellationToken);

        return building is null ? null : MapToBuildingDto(building);
    }

    public async Task<PagedResult<BuildingDto>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        await CheckClientExistAsync(clientId, cancellationToken);

        var result = await buildingRepository.GetBuildingByClientIdAsync(
            clientId,
            pagination,
            cancellationToken);

        return new PagedResult<BuildingDto>
        {
            Items = [.. result.Items.Select(MapToBuildingDto)],
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<BuildingDto> UpdateBuildingAsync(
        Guid id,
        UpdateBuildingRequest request,
        CancellationToken cancellationToken)
    {
        var building = await buildingRepository.GetBuildingByIdAsync(id, cancellationToken) ?? throw new NotFoundException("Building was not found.");

        await CheckCityExistAsync(request.CityId, cancellationToken);

        building.UpdateAddress(request.CityId, request.Street, request.Number);
        building.UpdateDetails(
            request.ConstructionYear,
            request.Type,
            request.NumberOfFloors,
            request.SurfaceArea,
            request.InsuredValue);
        building.UpdateRiskIndicators(
            request.IsFloodRiskZone,
            request.IsEarthquakeRiskZone);

        await buildingRepository.UpdateBuildingAsync(building, cancellationToken);

        return MapToBuildingDto(building);
    }

    private static BuildingDto MapToBuildingDto(Building building)
    {
        return new BuildingDto
        {
            Id = building.Id,
            ClientId = building.ClientId,
            CityId = building.CityId,
            Street = building.Street,
            Number = building.Number,
            ConstructionYear = building.ConstructionYear,
            Type = building.Type,
            NumberOfFloors = building.NumberOfFloors,
            SurfaceArea = building.SurfaceArea,
            InsuredValue = building.InsuredValue,
            IsFloodRiskZone = building.IsFloodRiskZone,
            IsEarthquakeRiskZone = building.IsEarthquakeRiskZone
        };
    }
}
