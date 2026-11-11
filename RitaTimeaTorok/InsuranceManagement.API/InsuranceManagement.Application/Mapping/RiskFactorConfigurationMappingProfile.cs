using AutoMapper;
using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Domain.Entities.Metadata;

namespace InsuranceManagement.Application.Mapping
{
    public class RiskFactorConfigurationMappingProfile : Profile
    {
        public RiskFactorConfigurationMappingProfile()
        {
            CreateMap<RiskFactorConfiguration, RiskFactorConfigurationDto>();
            CreateMap<CreateRiskFactorConfigurationDto, RiskFactorConfiguration>();
            CreateMap<UpdateRiskFactorConfigurationDto, RiskFactorConfiguration>();
        }
    }
}
