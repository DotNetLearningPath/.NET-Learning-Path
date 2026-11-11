using AutoMapper;
using InsuranceManagement.Application.DTOs.Metadata;
using InsuranceManagement.Domain.Entities.Metadata;

namespace InsuranceManagement.Application.Mapping
{
    public class CurrencyMappingProfile : Profile
    {
        public CurrencyMappingProfile()
        {
            CreateMap<Currency, CurrencyDto>();
            CreateMap<CreateCurrencyDto, Currency>();
        }
    }
}
