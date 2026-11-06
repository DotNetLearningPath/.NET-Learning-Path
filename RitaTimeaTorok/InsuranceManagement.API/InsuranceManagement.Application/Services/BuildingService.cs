using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services
{
    public class BuildingService(
        IBuildingRepository buildingRepository,
        IGenericRepository<Client> clientRepository,
        IGenericRepository<City> cityRepository,
        IGenericRepository<County> countyRepository,
        IGenericRepository<Country> countryRepository,
        IMapper mapper) : IBuildingService
    {
        public async Task<PagedResult<BuildingDto>> GetAllBuildingsAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await buildingRepository.GetPageAsync(pageNumber, pageSize);

            return new PagedResult<BuildingDto>
            {
                Items = mapper.Map<List<BuildingDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<BuildingDetailsDto> GetBuildingByIdAsync(Guid id)
        {
            var building = await buildingRepository.GetAsync(id) 
                ?? throw new EntityNotFoundException($"Building '{id}' was not found.");
            
            var city = await cityRepository.GetAsync(building.CityId)
                ?? throw new EntityNotFoundException("Building references a city that does not exist.");
            var county = await countyRepository.GetAsync(city.CountyId)
                ?? throw new EntityNotFoundException("City references a county that does not exist.");
            var country = await countryRepository.GetAsync(county.CountryId)
                ?? throw new EntityNotFoundException("County references a country that does not exist.");

            var buildingDetails = mapper.Map<BuildingDetailsDto>(building);
            buildingDetails.City = mapper.Map<CityDto>(city);
            buildingDetails.County = mapper.Map<CountyDto>(county);
            buildingDetails.Country = mapper.Map<CountryDto>(country);

            return buildingDetails;
        }

        public async Task<BuildingDto> CreateBuildingAsync(CreateBuildingDto request)
        {
            var buildingToCreate = mapper.Map<Building>(request);
            await ValidateBuildingAsync(buildingToCreate);

            var building = await buildingRepository.AddAsync(buildingToCreate);
            return mapper.Map<BuildingDto>(building);
        }

        public async Task<BuildingDto> UpdateBuildingAsync(Guid id, UpdateBuildingDto request)
        {
            var building = await buildingRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Building '{id}' was not found.");

            mapper.Map(request, building);
            await ValidateBuildingAsync(building);
            await buildingRepository.UpdateAsync(building);
            return mapper.Map<BuildingDto>(building);
        }

        public async Task DeleteBuildingAsync(Guid id)
        {
            var building = await buildingRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Building '{id}' was not found.");

            await buildingRepository.DeleteAsync(building);
        }

        private async Task ValidateBuildingAsync(Building building)
        {
            if (string.IsNullOrWhiteSpace(building.Street))
                throw new ValidationException("Building street is required.");

            if (building.StreetNumber <= 0)
                throw new ValidationException(
                    "Building street number must be greater than zero.");

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
}
