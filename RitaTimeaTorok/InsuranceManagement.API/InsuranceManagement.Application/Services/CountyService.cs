using AutoMapper;
using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.DTOs.Pagination;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Domain.Entities.Geography;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Application.Interfaces.Repositories;

namespace InsuranceManagement.Application.Services
{
    public class CountyService(
        IGenericRepository<County> countyRepository,
        IGenericRepository<Country> countryRepository,
        IMapper mapper) : ICountyService
    {
        public async Task<PagedResult<CountyDto>> GetAllCountiesAsync(int pageNumber, int pageSize)
        {
            PaginationValidator.Validate(pageNumber, pageSize);
            var page = await countyRepository.GetPageAsync(pageNumber, pageSize);

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
            return mapper.Map<CountyDto>(county);
        }

        public async Task<CountyDto> CreateCountyAsync(CreateCountyDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("County name is required.");

            if (await countryRepository.GetAsync(request.CountryId) is null)
                throw new ValidationException("CountryId must refer to an existing country.");

            var county = await countyRepository.AddAsync(mapper.Map<County>(request));
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
            return mapper.Map<CountyDto>(county);
        }

        public async Task DeleteCountyAsync(Guid id)
        {
            var county = await countyRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"County '{id}' was not found.");

            await countyRepository.DeleteAsync(county);
        }
    }
}
