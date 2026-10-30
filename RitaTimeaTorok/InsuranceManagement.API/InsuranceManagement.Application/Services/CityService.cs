using AutoMapper;
using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services;

public class CityService(
    IGenericRepository<City> cityService,
    IGenericRepository<County> countyService,
    IMapper mapper) : ICityService
{
    public async Task<List<CityDto>> GetAllAsync() =>
        mapper.Map<List<CityDto>>(await cityService.GetAllAsync());

    public async Task<CityDto?> GetAsync(Guid id) =>
        mapper.Map<CityDto?>(await cityService.GetAsync(id));

    public async Task<CityDto> CreateAsync(CreateCityDto request)
    {
        if (await countyService.GetAsync(request.CountyId) is null)
        {
            throw new ValidationException("CountyId must refer to an existing county.");
        }

        var city = await cityService.AddAsync(mapper.Map<City>(request));
        return mapper.Map<CityDto>(city);
    }

    public async Task<CityDto?> UpdateAsync(Guid id, UpdateCityDto request)
    {
        var city = await cityService.GetAsync(id);
        if (city is null) return null;

        if (await countyService.GetAsync(request.CountyId) is null)
        {
            throw new ValidationException("CountyId must refer to an existing county.");
        }

        mapper.Map(request, city);
        await cityService.UpdateAsync(city);
        return mapper.Map<CityDto>(city);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        if (await cityService.GetAsync(id) is null) return false;

        await cityService.DeleteAsync(id);
        return true;
    }
}
