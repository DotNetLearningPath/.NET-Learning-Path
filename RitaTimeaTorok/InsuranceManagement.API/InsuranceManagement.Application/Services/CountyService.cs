using AutoMapper;
using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services;

public class CountyService(
    IGenericRepository<County> countyService,
    IGenericRepository<Country> countryService,
    IMapper mapper) : ICountyService
{
    public async Task<List<CountyDto>> GetAllAsync() =>
        mapper.Map<List<CountyDto>>(await countyService.GetAllAsync());

    public async Task<CountyDto?> GetAsync(Guid id) =>
        mapper.Map<CountyDto?>(await countyService.GetAsync(id));

    public async Task<CountyDto> CreateAsync(CreateCountyDto request)
    {
        if (await countryService.GetAsync(request.CountryId) is null)
        {
            throw new ValidationException("CountryId must refer to an existing country.");
        }

        var county = await countyService.AddAsync(mapper.Map<County>(request));
        return mapper.Map<CountyDto>(county);
    }

    public async Task<CountyDto?> UpdateAsync(Guid id, UpdateCountyDto request)
    {
        var county = await countyService.GetAsync(id);
        if (county is null) return null;

        if (await countryService.GetAsync(request.CountryId) is null)
        {
            throw new ValidationException("CountryId must refer to an existing country.");
        }

        mapper.Map(request, county);
        await countyService.UpdateAsync(county);
        return mapper.Map<CountyDto>(county);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (await countyService.GetAsync(id) is null) return false;

        await countyService.DeleteAsync(id);
        return true;
    }
}
