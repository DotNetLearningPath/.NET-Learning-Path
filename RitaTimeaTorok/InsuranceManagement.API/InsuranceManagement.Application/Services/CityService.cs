using AutoMapper;
using System.ComponentModel.DataAnnotations;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Application.Interfaces;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Services;

public class CityService(
    IGenericRepository<City> cityRepository,
    IGenericRepository<County> countyRepository,
    IMapper mapper) : ICityService
{
    public async Task<List<CityDto>> GetAllAsync() =>
        mapper.Map<List<CityDto>>(await cityRepository.GetAllAsync());

    public async Task<CityDto?> GetAsync(Guid id) =>
        mapper.Map<CityDto?>(await cityRepository.GetAsync(id));

    public async Task<CityDto> CreateAsync(CreateCityDto request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("City name is required.");

        if (await countyRepository.GetAsync(request.CountyId) is null)
            throw new ValidationException("CountyId must refer to an existing county.");

        var city = await cityRepository.AddAsync(mapper.Map<City>(request));
        return mapper.Map<CityDto>(city);
    }

    public async Task<CityDto?> UpdateAsync(Guid id, UpdateCityDto request)
    {
        var city = await cityRepository.GetAsync(id);
        if (city is null) 
            return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidationException("City name is required.");

        if (await countyRepository.GetAsync(request.CountyId) is null)
            throw new ValidationException("CountyId must refer to an existing county.");

        mapper.Map(request, city);
        await cityRepository.UpdateAsync(city);
        return mapper.Map<CityDto>(city);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var city = await cityRepository.GetAsync(id);

        if (city is null)
            return false;

        await cityRepository.DeleteAsync(city);

        return true;
    }
}
