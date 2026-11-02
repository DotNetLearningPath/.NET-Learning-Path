using AutoMapper;
using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services;

public class CountyService(
    IGenericRepository<County> countyRepository,
    IGenericRepository<Country> countryRepository,
    IMapper mapper) : ICountyService
{
    public async Task<List<CountyDto>> GetAllAsync() =>
        mapper.Map<List<CountyDto>>(await countyRepository.GetAllAsync());

    public async Task<CountyDto?> GetAsync(Guid id) =>
        mapper.Map<CountyDto?>(await countyRepository.GetAsync(id));

    public async Task<CountyDto> CreateAsync(CreateCountyDto request)
    {
        if (await countryRepository.GetAsync(request.CountryId) is null)
        {
            throw new ValidationException("CountryId must refer to an existing country.");
        }

        var county = await countyRepository.AddAsync(mapper.Map<County>(request));
        return mapper.Map<CountyDto>(county);
    }

    public async Task<CountyDto?> UpdateAsync(Guid id, UpdateCountyDto request)
    {
        var county = await countyRepository.GetAsync(id);
        if (county is null) return null;

        if (await countryRepository.GetAsync(request.CountryId) is null)
        {
            throw new ValidationException("CountryId must refer to an existing country.");
        }

        mapper.Map(request, county);
        await countyRepository.UpdateAsync(county);
        return mapper.Map<CountyDto>(county);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var county = await countyRepository.GetAsync(id);

        if (county is null)
            return false;

        await countyRepository.DeleteAsync(county);

        return true;
    }
}
