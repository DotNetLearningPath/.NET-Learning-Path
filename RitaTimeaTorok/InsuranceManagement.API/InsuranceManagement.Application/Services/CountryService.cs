using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services
{
    public class CountryService(
        IGenericRepository<Country> countryRepository,
        ICountyRepository countyRepository,
        IMapper mapper) : ICountryService
    {
        public async Task<PagedResult<CountryDto>> GetAllCountriesAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await countryRepository.GetPageAsync(pageNumber, pageSize);

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
            return mapper.Map<CountryDto>(country);
        }

        public async Task<CountryDto> CreateCountryAsync(CreateCountryDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Country name is required.");

            var country = await countryRepository.AddAsync(mapper.Map<Country>(request));
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
            return mapper.Map<CountryDto>(country);
        }

        public async Task DeleteCountryAsync(Guid id)
        {
            var country = await countryRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Country '{id}' was not found.");

            if (await countyRepository.ExistsByCountryIdAsync(id))
                throw new ConflictException(
                    "Cannot delete a country that contains counties.");

            await countryRepository.DeleteAsync(country);
        }
    }
}
