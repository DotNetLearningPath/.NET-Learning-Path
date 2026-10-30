using AutoMapper;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services;

public class CountryService(
    IGenericRepository<Country> countryRepository,
    IMapper mapper) : ICountryService
{
    public async Task<List<CountryDto>> GetAllAsync() =>
        mapper.Map<List<CountryDto>>(await countryRepository.GetAllAsync());

    public async Task<CountryDto?> GetAsync(Guid id) =>
        mapper.Map<CountryDto?>(await countryRepository.GetAsync(id));

    public async Task<CountryDto> CreateAsync(CreateCountryDto request)
    {
        var country = await countryRepository.AddAsync(mapper.Map<Country>(request));
        return mapper.Map<CountryDto>(country);
    }

    public async Task<CountryDto?> UpdateAsync(Guid id, UpdateCountryDto request)
    {
        var country = await countryRepository.GetAsync(id);
        if (country is null) return null;

        mapper.Map(request, country);
        await countryRepository.UpdateAsync(country);
        return mapper.Map<CountryDto>(country);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (await countryRepository.GetAsync(id) is null) return false;

        await countryRepository.DeleteAsync(id);
        return true;
    }
}
