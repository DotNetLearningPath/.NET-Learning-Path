using AutoMapper;
using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class CityService(
        ICityRepository cityRepository,
        IGenericRepository<County> countyRepository,
        IBuildingRepository buildingRepository,
        IRiskFactorConfigurationRepository riskFactorConfigurationRepository,
        IMapper mapper,
        ILogger<CityService> logger) : ICityService
    {
        public async Task<PagedResult<CityDto>> GetAllCitiesAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await cityRepository.GetPageAsync(pageNumber, pageSize);
            logger.LogInformation(
                "Retrieved cities page {PageNumber} with page size {PageSize}; returned {ResultCount} of {TotalCount} cities",
                page.PageNumber, page.PageSize, page.Items.Count, page.TotalCount);

            return new PagedResult<CityDto>
            {
                Items = mapper.Map<List<CityDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<CityDto> GetCityByIdAsync(Guid id)
        {
            var city = await cityRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"City '{id}' was not found.");
            logger.LogInformation("Retrieved city {CityId}", id);
            return mapper.Map<CityDto>(city);
        }

        public async Task<CityDto> CreateCityAsync(CreateCityDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("City name is required.");

            if (await countyRepository.GetAsync(request.CountyId) is null)
                throw new ValidationException("CountyId must refer to an existing county.");

            var city = await cityRepository.AddAsync(mapper.Map<City>(request));
            
            logger.LogInformation("Created city {CityId}", city.Id);
            return mapper.Map<CityDto>(city);
        }

        public async Task<CityDto> UpdateCityAsync(Guid id, UpdateCityDto request)
        {
            var city = await cityRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"City '{id}' was not found.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("City name is required.");

            if (await countyRepository.GetAsync(request.CountyId) is null)
                throw new ValidationException("CountyId must refer to an existing county.");

            mapper.Map(request, city);
            await cityRepository.UpdateAsync(city);

            logger.LogInformation("Updated city {CityId}", city.Id);
            return mapper.Map<CityDto>(city);
        }

        public async Task DeleteCityAsync(Guid id)
        {
            var city = await cityRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"City '{id}' was not found.");

            if (await buildingRepository.ExistsByCityIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a city that contains buildings.");

            if (await riskFactorConfigurationRepository.ExistsByCityIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a city that has risk factor configurations.");

            await cityRepository.DeleteAsync(city);
            logger.LogInformation("Deleted city {CityId}", id);
        }
    }
}
