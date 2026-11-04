using AutoMapper;
using InsuranceManagement.Application.DTOs.Building;
using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Mapping;

public class BuildingMappingProfile : Profile
{
    public BuildingMappingProfile()
    {
        CreateMap<Building, BuildingDto>();
        CreateMap<CreateBuildingDto, Building>();
        CreateMap<UpdateBuildingDto, Building>();
    }
}
