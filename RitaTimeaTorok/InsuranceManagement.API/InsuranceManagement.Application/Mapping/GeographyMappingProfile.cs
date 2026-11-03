using AutoMapper;
using InsuranceManagement.Application.DTOs.Geography;
using InsuranceManagement.Domain.Entities.Geography;

namespace InsuranceManagement.Application.Mapping;

public class GeographyMappingProfile : Profile
{
    public GeographyMappingProfile()
    {
        CreateMap<Country, CountryDto>();
        CreateMap<CreateCountryDto, Country>();
        CreateMap<UpdateCountryDto, Country>();

        CreateMap<County, CountyDto>();
        CreateMap<CreateCountyDto, County>();
        CreateMap<UpdateCountyDto, County>();

        CreateMap<City, CityDto>();
        CreateMap<CreateCityDto, City>();
        CreateMap<UpdateCityDto, City>();
    }
}
