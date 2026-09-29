using Application.Abstractions;
using Application.DTO.Buildings;
using Application.DTO.Common;
using Application.Exceptions;
using Domain.Entities;

namespace Application.Services;

public class BuildingService : IBuildingService
{
    private readonly IBuildingRepository _buildingRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IGeographyRepository _geographyRepository;

    public BuildingService(
        IBuildingRepository buildingRepository,
        IClientRepository clientRepository,
        IGeographyRepository geographyRepository)
    {
        _buildingRepository = buildingRepository;
        _clientRepository = clientRepository;
        _geographyRepository = geographyRepository;
    }

    private async Task CheckClientExistAsync(Guid clientId, CancellationToken cancellationToken)
    {
        var client = await _clientRepository.GetClientByIdAsync(clientId, cancellationToken);

        if (client is null)
        {
            throw new NotFoundException("Client was not found.");
        }
    }

    private async Task CheckCityExistAsync(Guid cityId, CancellationToken cancellationToken)
    {
        var cityExists = await _geographyRepository.CityExistsAsync(cityId, cancellationToken);

        if (!cityExists)
        {
            throw new NotFoundException("City was not found.");
        }
    }

    public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingRequest request, CancellationToken cancellationToken = default)
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

        await _buildingRepository.AddBuildingAsync(building, cancellationToken);

        return MapToBuildingDto(building);
    }

    public async Task<BuildingDto?> GetBuildingByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var building = await _buildingRepository.GetBuildingByIdAsync(id, cancellationToken);

        return building is null ? null : MapToBuildingDto(building);
    }

    public async Task<PagedResult<BuildingDto>> GetBuildingByClientIdAsync(
        Guid clientId,
        PaginationRequest pagination,
        CancellationToken cancellationToken = default)
    {
        await CheckClientExistAsync(clientId, cancellationToken);

        var result = await _buildingRepository.GetBuildingByClientIdAsync(
            clientId,
            pagination,
            cancellationToken);

        return new PagedResult<BuildingDto>
        {
            Items = result.Items.Select(MapToBuildingDto).ToList(),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount
        };
    }

    public async Task<BuildingDto> UpdateBuildingAsync(
        Guid id,
        UpdateBuildingRequest request,
        CancellationToken cancellationToken = default)
    {
        var building = await _buildingRepository.GetBuildingByIdAsync(id, cancellationToken);

        if (building is null)
        {
            throw new NotFoundException("Building was not found.");
        }

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

        await _buildingRepository.UpdateBuildingAsync(building, cancellationToken);

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
