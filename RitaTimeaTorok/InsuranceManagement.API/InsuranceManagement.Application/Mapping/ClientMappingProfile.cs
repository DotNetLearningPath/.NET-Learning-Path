using AutoMapper;
using InsuranceManagement.Application.DTOs.Client;
using InsuranceManagement.Domain.Entities;

namespace InsuranceManagement.Application.Mapping;

public class ClientMappingProfile : Profile
{
    public ClientMappingProfile()
    {
        CreateMap<Client, ClientDto>();
        CreateMap<CreateClientDto, Client>();
        CreateMap<UpdateClientDto, Client>();
    }
}
