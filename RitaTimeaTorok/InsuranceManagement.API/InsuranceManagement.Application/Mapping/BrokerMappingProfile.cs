using AutoMapper;
using InsuranceManagement.Application.DTOs.Broker;
using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Mapping
{
    public class BrokerMappingProfile : Profile
    {
        public BrokerMappingProfile()
        {
            CreateMap<Broker, BrokerDto>();
            CreateMap<CreateBrokerDto, Broker>();
        }
    }
}
