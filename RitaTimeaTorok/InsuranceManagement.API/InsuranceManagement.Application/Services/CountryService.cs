using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities.Geography;
using Microsoft.Extensions.Logging;

namespace InsuranceManagement.Application.Services
{
    public class CountryService(
        IGenericRepository<Country> countryRepository,
        ICountyRepository countyRepository,
        IRiskFactorConfigurationRepository riskFactorConfigurationRepository,
        IMapper mapper,
        ILogger<CountryService> logger) : ICountryService
    {
        public async Task<PagedResult<CountryDto>> GetAllCountriesAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await countryRepository.GetPageAsync(pageNumber, pageSize);
            logger.LogInformation(
                "Retrieved countries page {PageNumber} with page size {PageSize}; returned {ResultCount} of {TotalCount} countries",
                page.PageNumber, page.PageSize, page.Items.Count, page.TotalCount);

            return new PagedResult<CountryDto>
            {
                Items = mapper.Map<List<CountryDto>>(page.Items),
                PageNumber = page.PageNumber,
                PageSize = page.PageSize,
                TotalCount = page.TotalCount
            };
        }

        public async Task<CountryDto> GetCountryByIdAsync(Guid id)
        {
            var country = await countryRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Country '{id}' was not found.");
            logger.LogInformation("Retrieved country {CountryId}", id);
            return mapper.Map<CountryDto>(country);
        }

        public async Task<CountryDto> CreateCountryAsync(CreateCountryDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Country name is required.");

            var country = await countryRepository.AddAsync(mapper.Map<Country>(request));
            logger.LogInformation("Created country {CountryId}", country.Id);
            return mapper.Map<CountryDto>(country);
        }

        public async Task<CountryDto> UpdateCountryAsync(Guid id, UpdateCountryDto request)
        {
            var country = await countryRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Country '{id}' was not found.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Country name is required.");

            mapper.Map(request, country);
            await countryRepository.UpdateAsync(country);
            logger.LogInformation("Updated country {CountryId}", country.Id);
            return mapper.Map<CountryDto>(country);
        }

        public async Task DeleteCountryAsync(Guid id)
        {
            var country = await countryRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Country '{id}' was not found.");

            if (await countyRepository.ExistsByCountryIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a country that contains counties.");

            if (await riskFactorConfigurationRepository.ExistsByCountryIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a country that has risk factor configurations.");

            await countryRepository.DeleteAsync(country);
            logger.LogInformation("Deleted country {CountryId}", id);
        }
    }
}
