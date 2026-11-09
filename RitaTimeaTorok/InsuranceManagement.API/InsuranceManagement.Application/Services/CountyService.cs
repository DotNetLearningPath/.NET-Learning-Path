using AutoMapper;
using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Application.Interfaces.Repositories;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class CountyService(
        ICountyRepository countyRepository,
        IGenericRepository<Country> countryRepository,
        ICityRepository cityRepository,
        IMapper mapper,
        ILogger<CountyService> logger) : ICountyService
    {
        public async Task<PagedResult<CountyDto>> GetAllCountiesAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await countyRepository.GetPageAsync(pageNumber, pageSize);
            logger.LogInformation(
                "Retrieved counties page {PageNumber} with page size {PageSize}; returned {ResultCount} of {TotalCount} counties",
                page.PageNumber, page.PageSize, page.Items.Count, page.TotalCount);

            return new PagedResult<CountyDto>
            {
                Items = mapper.Map<List<CountyDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<CountyDto> GetCountyByIdAsync(Guid id)
        {
            var county = await countyRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"County '{id}' was not found.");
            logger.LogInformation("Retrieved county {CountyId}", id);
            return mapper.Map<CountyDto>(county);
        }

        public async Task<CountyDto> CreateCountyAsync(CreateCountyDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("County name is required.");

            if (await countryRepository.GetAsync(request.CountryId) is null)
                throw new ValidationException("CountryId must refer to an existing country.");

            var county = await countyRepository.AddAsync(mapper.Map<County>(request));
            logger.LogInformation("Created county {CountyId}", county.Id);
            return mapper.Map<CountyDto>(county);
        }

        public async Task<CountyDto> UpdateCountyAsync(Guid id, UpdateCountyDto request)
        {
            var county = await countyRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"County '{id}' was not found.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("County name is required.");

            if (await countryRepository.GetAsync(request.CountryId) is null)
                throw new ValidationException("CountryId must refer to an existing country.");

            mapper.Map(request, county);
            await countyRepository.UpdateAsync(county);
            logger.LogInformation("Updated county {CountyId}", county.Id);
            return mapper.Map<CountyDto>(county);
        }

        public async Task DeleteCountyAsync(Guid id)
        {
            var county = await countyRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"County '{id}' was not found.");

            if (await cityRepository.ExistsByCountyIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a county that contains cities.");

            await countyRepository.DeleteAsync(county);
            logger.LogInformation("Deleted county {CountyId}", id);
        }
    }
}
