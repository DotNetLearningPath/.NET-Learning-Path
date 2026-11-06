using AutoMapper;
using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Exceptions;

namespace InsuranceManagement.Application.Services
{
    public class CityService(
        IGenericRepository<City> cityRepository,
        IGenericRepository<County> countyRepository,
        IBuildingRepository buildingRepository,
        IMapper mapper) : ICityService
    {
        public async Task<PagedResult<CityDto>> GetAllCitiesAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await cityRepository.GetPageAsync(pageNumber, pageSize);

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
            
            return mapper.Map<CityDto>(city);
        }

        public async Task<CityDto> CreateCityAsync(CreateCityDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("City name is required.");

            if (await countyRepository.GetAsync(request.CountyId) is null)
                throw new ValidationException("CountyId must refer to an existing county.");

            var city = await cityRepository.AddAsync(mapper.Map<City>(request));
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
            return mapper.Map<CityDto>(city);
        }

        public async Task DeleteCityAsync(Guid id)
        {
            var city = await cityRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"City '{id}' was not found.");

            if (await buildingRepository.ExistsByCityIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a city that contains buildings.");

            await cityRepository.DeleteAsync(city);

        }
    }
}
