using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services;

public class BuildingService(
    IBuildingRepository buildingRepository,
    IGenericRepository<Client> clientRepository,
    IGenericRepository<City> cityRepository,
    IMapper mapper) : IBuildingService
{
    public async Task<List<BuildingDto>> GetAllAsync() =>
        mapper.Map<List<BuildingDto>>(await buildingRepository.GetAllAsync());

    public async Task<BuildingDto?> GetAsync(Guid id) =>
        mapper.Map<BuildingDto?>(await buildingRepository.GetAsync(id));

    public async Task<BuildingDto> CreateAsync(CreateBuildingDto request)
    {
        var buildingToCreate = mapper.Map<Building>(request);
        await ValidateBuildingAsync(buildingToCreate);

        var building = await buildingRepository.AddAsync(buildingToCreate);
        return mapper.Map<BuildingDto>(building);
    }

    public async Task<BuildingDto?> UpdateAsync(Guid id, UpdateBuildingDto request)
    {
        var building = await buildingRepository.GetAsync(id);
        if (building is null)
            return null;

        var buildingToUpdate = mapper.Map<Building>(request);
        await ValidateBuildingAsync(buildingToUpdate);
        mapper.Map(request, building);
        await buildingRepository.UpdateAsync(building);
        return mapper.Map<BuildingDto>(building);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var building = await buildingRepository.GetAsync(id);
        if (building is null)
            return false;

        await buildingRepository.DeleteAsync(building);
        return true;
    }

    private async Task ValidateBuildingAsync(Building building)
    {
        if (string.IsNullOrWhiteSpace(building.Address))
            throw new ValidationException("Building address is required.");

        if (building.ConstructionYear <= 0)
            throw new ValidationException(
                "Construction year must be positive.");

        if (building.NumberOfFloors < 0)
            throw new ValidationException(
                "Number of floors cannot be negative.");

        if (building.SurfaceArea <= 0)
            throw new ValidationException(
                "Surface area must be positive.");

        if (building.InsuredValue <= 0)
            throw new ValidationException(
                "Insured value must be positive.");

        if (await clientRepository.GetAsync(building.ClientId) is null)
            throw new ValidationException(
                "ClientId must refer to an existing client.");

        if (await cityRepository.GetAsync(building.CityId) is null)
            throw new ValidationException(
                "CityId must refer to an existing city.");
    }
}
