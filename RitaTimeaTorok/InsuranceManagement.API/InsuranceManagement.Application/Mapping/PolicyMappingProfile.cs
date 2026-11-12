using AutoMapper;
using InsuranceManagement.Application.DTOs.Policy;
using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Mapping
{
    public class PolicyMappingProfile : Profile
    {
        public PolicyMappingProfile()
        {
            CreateMap<Policy, PolicyDto>();
            CreateMap<Policy, PolicyListItemDto>();
            CreateMap<PolicyCalculationSnapshot, PolicyCalculationSnapshotDto>();
        }
    }
}
