using AutoMapper;
using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Domain.Entities.Metadata;

namespace InsuranceManagement.Application.Mapping
{
    public class FeeConfigurationMappingProfile : Profile
    {
        public FeeConfigurationMappingProfile()
        {
            CreateMap<FeeConfiguration, FeeConfigurationDto>();
            CreateMap<CreateFeeConfigurationDto, FeeConfiguration>();
        }
    }
}
