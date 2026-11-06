using System.ComponentModel.DataAnnotations;
using AutoMapper;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Exceptions;
using InsuranceManagement.Application.Interfaces.Repositories;
using InsuranceManagement.Application.Interfaces.Services;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services
{
    public class CountryService(
        IGenericRepository<Country> countryRepository,
        IMapper mapper) : ICountryService
    {
        public async Task<List<CountryDto>> GetAllAsync() =>
            mapper.Map<List<CountryDto>>(await countryRepository.GetAllAsync());

        public async Task<CountryDto> GetAsync(Guid id)
        {
            var country = await countryRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Country '{id}' was not found.");
            return mapper.Map<CountryDto>(country);
        }

        public async Task<CountryDto> CreateAsync(CreateCountryDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Country name is required.");

            var country = await countryRepository.AddAsync(mapper.Map<Country>(request));
            return mapper.Map<CountryDto>(country);
        }

        public async Task<CountryDto> UpdateAsync(Guid id, UpdateCountryDto request)
        {
            var country = await countryRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Country '{id}' was not found.");

            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ValidationException("Country name is required.");

            mapper.Map(request, country);
            await countryRepository.UpdateAsync(country);
            return mapper.Map<CountryDto>(country);
        }

        public async Task DeleteAsync(Guid id)
        {
            var country = await countryRepository.GetAsync(id)
                ?? throw new EntityNotFoundException($"Country '{id}' was not found.");

            await countryRepository.DeleteAsync(country);
        }
    }
}
